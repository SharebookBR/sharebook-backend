using FluentValidation;
using Flurl;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IList<string>> FetchMeetupsAsync()
        {
            var logs = new List<string>();

            if (!_settings.IsActive) throw new Exception("O Serviço de busca de meetups está desativado no appSettings.");

            var newMeetups = await GetMeetupsFromSymplaAsync();
            var newYoutubeVideos = await GetYoutubeVideosAsync();

            await SyncMeetupParticipantsListAsync(logs);

            logs.Add($"Foram encontradas {newMeetups} novas meetups e {newYoutubeVideos} novos vídeos relacionados");

            return logs;
        }

        private async Task SyncMeetupParticipantsListAsync(IList<string> logs)
        {
            // Carrega os inscritos no evento um dia após o evento ser feito. Carrega apenas 5 para poupar recursos.
            var meetups = _repository.Get().Where(x => x.StartDate < DateTime.Now.AddDays(1) && !x.IsParticipantListSynced).Take(5).ToList();

            logs.Add($"Sincronizando inscritos nos meetups. Encontrei {meetups.Count} meetups pra sincronizar.");

            foreach (var meetup in meetups)
            {
                // evento não teve inscrição no sympla. Raro mas aconteceu.
                if (meetup.SymplaEventId == -1) continue;

                var meetupParticipants = await GetMeetupParticipantsAsync(meetup.SymplaEventId);
                foreach (var participant in meetupParticipants)
                {
                    participant.Meetup = meetup;
                    await _participantRepository.InsertAsync(participant);
                }
                meetup.IsParticipantListSynced = true;
                await _repository.UpdateAsync(meetup);

                logs.Add($"Adicionei {meetupParticipants.Count} inscritos no meetup '{meetup.Title}'.");
            }
        }

        private async Task<int> GetYoutubeVideosAsync()
        {
            var meetups = await _repository.GetAsync(x => x.YoutubeUrl == null, x => x.StartDate);

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
                // TODO: Verify if it's possible to use Task.WhenAll or similar
                updatedMeetups.ForEach(async(m) => await _repository.UpdateAsync(m));
            }

            return updatedMeetups.Count;
        }

        private async Task<int> GetMeetupsFromSymplaAsync()
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
                                field_sort = "start_date",
                                sort = "desc"
                            })
                            .GetJsonAsync<SymplaDto>();
                foreach (var symplaEvent in symplaDto.Data)
                {
                    if (!await _repository.AnyAsync(s => s.SymplaEventId == symplaEvent.Id))
                    {
                        var coverUrl = await UploadCoverAsync(symplaEvent.Image, symplaEvent.Name);

                        await _repository.InsertAsync(new Meetup
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
        private async Task<IList<MeetupParticipant>> GetMeetupParticipantsAsync(int eventId)
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

        private async Task<string> UploadCoverAsync(string coverUrl, string eventName)
        {
            var imageBytes = await GetCoverImageBytesAsync(coverUrl);

            var resizedImageBytes = ImageHelper.ResizeImage(imageBytes, 50);

            var fileName = new Uri(coverUrl).Segments.Last();

            var imageSlug = eventName.GenerateSlug();

            var imageName = ImageHelper.FormatImageName(fileName, imageSlug);

            return await _uploadService.UploadImageAsync(resizedImageBytes, imageName, "Meetup");
        }

        public async Task<IList<Meetup>> SearchAsync(string criteria)
        {
            return await _repository.Get()
                .Where(m => m.Active && (m.Title.Contains(criteria) || m.Description.Contains(criteria)))
                .OrderByDescending(m => m.CreationDate)
                .ToListAsync();
        }
    }
}
