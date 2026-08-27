namespace ShareBook.Service.Upload;

public class BookThumbnailBackfillResult
{
    public int SourceFiles { get; set; }
    public int Offset { get; set; }
    public int Processed { get; set; }
    public bool HasMore { get; set; }
    public int? NextOffset { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public long SourceBytes { get; set; }
    public long ThumbnailBytes { get; set; }
    public IList<BookThumbnailBackfillFailure> Failures { get; set; } = new List<BookThumbnailBackfillFailure>();
}

public class BookThumbnailBackfillFailure
{
    public string FileName { get; set; }
    public string Error { get; set; }
}
