namespace ShareBook.Service.Upload
{
    public interface IUploadService
    {
        void UploadImage(byte[] imageBytes, string imageName);
    }
}
