namespace EquipLink.Helpers
{
    // IFileUploadService.cs
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<bool> DeleteFileAsync(string filePath);
        bool FileExists(string filePath);
    }
}
