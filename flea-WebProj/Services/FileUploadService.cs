namespace flea_WebProj.Services;

public interface IFileUploadService
{
    Task<(bool success, string? filePath, string? error)> UploadImageAsync(IFormFile file, string folder = "products");
    Task<List<(bool success, string? filePath, string? error)>> UploadMultipleImagesAsync(List<IFormFile> files, string folder = "products");
    bool DeleteImage(string filePath);
    bool IsValidImage(IFormFile file);
}

public class FileUploadService
{
    
}