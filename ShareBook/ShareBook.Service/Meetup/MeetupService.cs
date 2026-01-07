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

namespace ShareBook.Service;

public class MeetupService : BaseService<Meetup>, IMeetupService
{
    private readonly MeetupSettings _settings;
    private readonly IUploadService _uploadService;
    protected readonly ApplicationDbContext _context;

    public MeetupService(IOptions<MeetupSettings> settings, IMeetupRepository meetupRepository, ApplicationDbContext context, IUnitOfWork unitOfWork, IValidator<Meetup> validator, IUploadService uploadService) : base(meetupRepository, unitOfWork, validator)
    {
        _settings = settings.Value;
        _uploadService = uploadService;
        _context = context;
    }

    public async Task<IList<string>> FetchMeetupsAsync()
    {
        var logs = new List<string>();

        if (!_settings.IsActive) throw new Exception("O Serviço de busca de meetups está desativado no appSettings.");

        var newYoutubeVideos = await GetYoutubeVideosAsync();

        foreach (var video in newYoutubeVideos.Items)
        {
            var title = video.Snippet.Title;
            var meetupsFromDb = await SearchAsync(title);

            if (meetupsFromDb.Count == 0)
            {
                var meetup = new Meetup
                {
                    Cover = video.Snippet.Thumbnails.Medium.Url,
                    Title = title,
                    YoutubeUrl = $"https://www.youtube.com/watch?v={video.Id.VideoId}"
                };

                // alguns detalhes do vídeo precisamos pegar em outro endpoint da api do you tube
                var videoDetails = await GetYoutubeVideoDetailsAsync(video.Id.VideoId);
                meetup.Description = videoDetails.Items[0].Snippet.Description;
                meetup.StartDate = videoDetails.Items[0].liveStreamingDetails?.scheduledStartTime ?? videoDetails.Items[0].Snippet.PublishedAt;

                _context.Meetups.Add(meetup);
                await _context.SaveChangesAsync();

                logs.Add($"Adicionei o vídeo '{title}'  no banco de dados.");

            }
            else
            {
                logs.Add($"O vídeo '{title}' já estava no banco de dados. Não fiz nada.");
                logs.Add($"Paranda de carregar meetups. Apenas o delta interessa. ( poupando cota api you tube )");
                break;
            }

        }


        return logs;
    }

    private async Task<YoutubeDto> GetYoutubeVideosAsync(int level = 1, string pageToken = "")
    {
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
                    pageToken
                }).GetJsonAsync<YoutubeDto>();

            // cada requisição trás apenas 5 vídeos. Vamos fazer 3 requisições pra ter uma amostragem boa.
            if (level <= 3)
            {
                level++;
                var youtubeDto2 = await GetYoutubeVideosAsync(level, youtubeDto.nextPageToken);
                youtubeDto.Items.AddRange(youtubeDto2.Items);
            }
        }
        catch (FlurlHttpException e)
        {
            var dtoError = await e.GetResponseJsonAsync<YoutubeDtoError>();
            throw new ShareBookException(dtoError == null ? e.Message : dtoError.error.Message);
        }

        return youtubeDto;
    }

    private async Task<YoutubeDtoDetail> GetYoutubeVideoDetailsAsync(string id)
    {
        YoutubeDtoDetail youtubeDto = new YoutubeDtoDetail();


        try
        {
            youtubeDto = await "https://youtube.googleapis.com/youtube/v3/videos"
                .SetQueryParams(new
                {
                    key = _settings.YoutubeToken,
                    part = "snippet, liveStreamingDetails",
                    id
                }).GetJsonAsync<YoutubeDtoDetail>();


        }
        catch (FlurlHttpException e)
        {
            var dtoError = await e.GetResponseJsonAsync<YoutubeDtoError>();
            throw new ShareBookException(dtoError == null ? e.Message : dtoError.error.Message);
        }

        return youtubeDto;
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

    public async Task<IList<Meetup>> SearchAsync(string title)
    {
        return await _repository.Get()
            .Where(m => m.Active && (m.Title.Contains(title) || m.Description.Contains(title)))
            .OrderByDescending(m => m.StartDate)
            .ToListAsync();
    }
}
