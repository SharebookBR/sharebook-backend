using System.Collections.Generic;

namespace ShareBook.Service.Dto;

public class YoutubeDtoError
{
    public Error error { get; set; }
}

public class Error
{
    public int Code { get; set; }
    public string Message { get; set; }
    public List<YoutubeErrorDetailDto> Errors { get; set; }
    public string Status { get; set; }
}

public class YoutubeErrorDetailDto
{
    public string Message { get; set; }
    public string Domain { get; set; }
    public string Reason { get; set; }
}
