using FluentValidation;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Domain.Exceptions;
using ShareBook.Helper.Extensions;
using ShareBook.Helper.Image;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Dto;
using ShareBook.Service.Generic;
using ShareBook.Service.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class MeetupService : BaseService<Meetup>, IMeetupService
    {
        private readonly MeetupSettings _settings;
        private readonly IUploadService _uploadService;
        private readonly IMeetupParticipantRepository _participantRepository;

        public MeetupService(IOptions<MeetupSettings> settings, IMeetupRepository meetupRepository, IMeetupParticipantRepository meetupParticipantRepository, IUnitOfWork unitOfWork, IValidator<Meetup> validator, IUploadService uploadService) : base(meetupRepository, unitOfWork, validator)
        {
            _settings = settings.Value;
            _uploadService = uploadService;
            _participantRepository = meetupParticipantRepository;
        }

        public async Task<IList<string>> FetchMeetups()
        {
            var logs = new List<string>();

            if (!_settings.IsActive) throw new Exception("O Serviço de busca de meetups está desativado no appSettings.");

            var newMeetups = await GetMeetupsFromSympla();
            var newYoutubeVideos = await GetYoutubeVideos();

            await SyncMeetupParticipantsList(logs);

            logs.Add($"Foram encontradas {newMeetups} novas meetups e {newYoutubeVideos} novos vídeos relacionados");

            return logs;
        }

        private async Task SyncMeetupParticipantsList(IList<string> logs)
        {
            // Carrega os inscritos no evento um dia após o evento ser feito. Carrega apenas 5 para poupar recursos.
            var meetups = _repository.Get().Where(x => x.StartDate < DateTime.Now.AddDays(1) && !x.IsParticipantListSynced).Take(5).ToList();

            logs.Add($"Sincronizando inscritos nos meetups. Encontrei {meetups.Count} meetups pra sincronizar.");

            foreach (var meetup in meetups)
            {
                // evento não teve inscrição no sympla. Raro mas aconteceu.
                if (meetup.SymplaEventId == -1) continue;

                var meetupParticipants = await GetMeetupParticipants(meetup.SymplaEventId);
                foreach (var participant in meetupParticipants)
                {
                    participant.Meetup = meetup;
                    _participantRepository.Insert(participant);
                }
                meetup.IsParticipantListSynced = true;
                _repository.Update(meetup);

                logs.Add($"Adicionei {meetupParticipants.Count} inscritos no meetup '{meetup.Title}'.");
            }
        }

        private async Task<int> GetYoutubeVideos()
        {
            var meetups = _repository.Get(x => x.YoutubeUrl == null, x => x.StartDate);

            if (meetups.TotalItems == 0) return 0;

            YoutubeDto youtubeDto;
            try
            {
                youtubeDto = await "https://youtube.googleapis.com/youtube/v3/search"
                    .SetQueryParams(new
                    {
                        key = _settings.YoutubeToken,
                        part = "snippet",
                        type = "video",
                        channelId = "UCPEWmRDlhOJHac6Fk-MwGBQ",
                        order = "date",
                    }).GetJsonAsync<YoutubeDto>();
            }
            catch (FlurlHttpException e)
            {
                var error = await e.GetResponseJsonAsync<YoutubeDto>();

                throw new ShareBookException(error == null ? e.Message : error.Message);
            }

            var updatedMeetups = meetups.Items.Join(youtubeDto.Items,
                                    m => m.Title,
                                    y => y.Snippet.Title,
                                    (m, y) => { m.YoutubeUrl = $"https://youtube.com/watch?v={y.Id.VideoId}"; return m; }).ToList();

            if (updatedMeetups.Any())
            {
                updatedMeetups.ForEach(m => _repository.Update(m));
            }

            return updatedMeetups.Count;
        }

        private async Task<int> GetMeetupsFromSympla()
        {
            int eventsAdded = 0;
            SymplaDto symplaDto;
            try
            {
                symplaDto = await "https://api.sympla.com.br/public/v3/events"
                            .WithHeader("s_token", _settings.SymplaToken)
                            .SetQueryParams(new
                            {
                                //page_size = 10,
                                field_sort = "start_date"
                            })
                            .GetJsonAsync<SymplaDto>();
                foreach (var symplaEvent in symplaDto.Data)
                {
                    if (!_repository.Any(s => s.SymplaEventId == symplaEvent.Id))
                    {
                        var coverUrl = await UploadCover(symplaEvent.Image, symplaEvent.Name);

                        _repository.Insert(new Meetup
                        {
                            SymplaEventId = symplaEvent.Id,
                            SymplaEventUrl = symplaEvent.Url,
                            Title = symplaEvent.Name,
                            Cover = coverUrl,
                            Description = symplaEvent.Detail,
                            StartDate = DateTime.Parse(symplaEvent.StartDate),
                        });
                        eventsAdded++;
                    }
                }
            }
            catch (FlurlHttpException e)
            {
                var error = await e.GetResponseJsonAsync<SymplaDto>();

                throw new ShareBookException(error == null ? e.Message : error.Message);
            }

            return eventsAdded;
        }
        private async Task<IList<MeetupParticipant>> GetMeetupParticipants(int eventId)
        {
            IList<MeetupParticipant> participants = new List<MeetupParticipant>();
            int page = 1;
            bool hasNext = true;

            while (hasNext)
            {
                var participantDto = await $"https://api.sympla.com.br/public/v3/events/{eventId}/participants"
                                .WithHeader("s_token", _settings.SymplaToken)
                                .SetQueryParams(new
                                {
                                    page = page,
                                })
                                .GetJsonAsync<MeetupParticipantDto>();

                foreach (var participant in participantDto.Data)
                {
                    participants.Add(new MeetupParticipant
                    {
                        FirstName = participant.FirstName,
                        LastName = participant.LastName,
                        Email = participant.Email,
                    });
                }
                
                hasNext = participantDto.Pagination.HasNext;
                if (hasNext) page++;                
            }
            return participants;
        }

        private static async Task<byte[]> GetCoverImageBytesAsync(string url)
        {
            try
            {
                return await url.GetBytesAsync();
            }
            catch (FlurlHttpException e)
            {
                throw new ShareBookException($"{e.StatusCode}: Falha ao obter imagem do Meetup");
            }
        }

        private async Task<string> UploadCover(string coverUrl, string eventName)
        {
            var imageBytes = await GetCoverImageBytesAsync(coverUrl);

            var resizedImageBytes = ImageHelper.ResizeImage(imageBytes, 50);

            var fileName = new Uri(coverUrl).Segments.Last();

            var imageSlug = eventName.GenerateSlug();

            var imageName = ImageHelper.FormatImageName(fileName, imageSlug);

            return _uploadService.UploadImage(resizedImageBytes, imageName, "Meetup");
        }

        public IList<Meetup> Search(string criteria)
        {
            return _repository.Get()
                .Where(m => m.Active && ( m.Title.ToUpper().Contains(criteria.ToUpper()) || m.Description.ToUpper().Contains(criteria.ToUpper())))
                .OrderByDescending(m => m.CreationDate)
                .ToList();
        }
    }
}
