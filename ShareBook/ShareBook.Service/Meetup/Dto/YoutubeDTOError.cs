using System.Collections.Generic;

namespace ShareBook.Service.Dto;

public class YoutubeDtoError
{
    public Error error { get; set; } = null!;
}

public class Error
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<YoutubeErrorDetailDto> Errors { get; set; } = [];
    public string Status { get; set; } = string.Empty;
}

public class YoutubeErrorDetailDto
{
    public string Message { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
