namespace ShareBook.Service.Upload
{
    public interface IUploadService
    {
        string UploadImage(byte[] imageBytes, string imageName, string lastDirectory);
        string GetImageUrl(string imageName, string lastDirectory);
    }
}
