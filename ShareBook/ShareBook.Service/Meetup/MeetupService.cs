using FluentValidation;
using Microsoft.Extensions.Configuration;
using ShareBook.Domain;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using ShareBook.Service.Dto;
using ShareBook.Helper.String;
using System.Net.Http;

namespace ShareBook.Service
{
    public class MeetupService : BaseService<Meetup>, IMeetupService
    {
        private readonly MeetupSettings _settings;
        public MeetupService(IOptions<MeetupSettings> settings, IMeetupRepository meetupRepository, IUnitOfWork unitOfWork, IValidator<Meetup> validator) : base(meetupRepository, unitOfWork, validator)
        {
            _settings = settings.Value;
        }
        public async Task<string> FetchMeetups()
        {
            if (!_settings.IsActive) throw new Exception("O Serviço de busca de meetups está desativado no appSettings.");
            
            var newMeetups = await GetMeetupsFromSympla();
            var newYoutubeVideos = await GetYoutubeVideos();

            return $"Foram encontradas {newMeetups} novas meetups e {newYoutubeVideos} novos vídeos relacionados";
        }

        private async Task<int> GetYoutubeVideos()
        {
            var meetups = _repository.Get(x => x.YoutubeUrl == null, x => x.StartDate);

            if (meetups.TotalItems == 0) return 0;

            int videosFound = 0;
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

            // Ignorar tudo com menos de 0.85% de similaridade.
            double similarityThreshold = 0.85;

            foreach (var meetup in meetups.Items)
            {
                var similarityDictionary = new Dictionary<Item, double>();

                foreach (var videoItem in youtubeDto.Items)
                {
                    double similarity = StringHelper.CalculateSimilarity(meetup.Title, videoItem.Snippet.Title);
                    if (similarity >= similarityThreshold)
                    {
                        similarityDictionary.Add(videoItem, similarity);
                    }
                }

                var bestMatch = similarityDictionary.FirstOrDefault(x => x.Value == similarityDictionary.Values.Max());

                meetup.YoutubeUrl = $"https://youtube.com/watch?v={bestMatch.Key.Id.VideoId}";

                _repository.Update(meetup);

                videosFound++;
            }

            return videosFound;
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
                        _repository.Insert(new Meetup
                        {
                            SymplaEventId = symplaEvent.Id,
                            SymplaEventUrl = symplaEvent.Url,
                            Title = symplaEvent.Name,
                            Cover = symplaEvent.Image,
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
    }
}
