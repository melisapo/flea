namespace flea_WebProj.Services;

public interface IFileUploadService
{
    Task<(bool success, string? filePath, string? error)> UploadImageAsync(IFormFile file, string folder = "products");
    Task<List<(bool success, string? filePath, string? error)>> UploadMultipleImagesAsync(List<IFormFile> files, string folder = "products");
    bool DeleteImage(string filePath);
    bool IsValidImage(IFormFile file);
}

public class FileUploadService(IWebHostEnvironment environment) : IFileUploadService
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5mb
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public async Task<(bool success, string? filePath, string? error)> UploadImageAsync(IFormFile file, string folder = "products")
    {
        try
        {
            // Validar archivo
            if (!IsValidImage(file))
                return (false, null, "Archivo inválido. Solo se permiten imágenes JPG, PNG, GIF o WebP de máximo 5MB.");
            
            var uploadFolder = Path.Combine(environment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // Generar nombre único para el archivo
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            // Guardar archivo
            await using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            // Retornar ruta relativa (para guardar en BD)
            var relativePath = $"/uploads/{folder}/{uniqueFileName}";
            return (true, relativePath, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"Error al subir imagen: {ex.Message}");
        }
    }

    public async Task<List<(bool success, string? filePath, string? error)>> UploadMultipleImagesAsync(List<IFormFile> files, string folder = "products")
    {
        var results = new List<(bool success, string? filePath, string? error)>();

        foreach (var file in files)
        {
            var result = await UploadImageAsync(file, folder);
            results.Add(result);
        }

        return results;
    }

    public bool DeleteImage(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;
            
            var fullPath = Path.Combine(environment.WebRootPath, filePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (!File.Exists(fullPath)) return false;
            
            File.Delete(fullPath);
            return true;

        }
        catch
        {
            return false;
        }
    }

    public bool IsValidImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return false;
        
        if (file.Length > MaxFileSize)
            return false;
        
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return false;

        var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        return allowedMimeTypes.Contains(file.ContentType.ToLower());
    }
}