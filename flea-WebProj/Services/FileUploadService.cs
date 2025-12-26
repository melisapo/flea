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
    private readonly IWebHostEnvironment _environment = environment;
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5mb
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public async Task<(bool success, string? filePath, string? error)> UploadImageAsync(IFormFile file, string folder = "products")
    {
        try
        {
            // Validar archivo
            if (!IsValidImage(file))
            {
                return (false, null, "Archivo inválido. Solo se permiten imágenes JPG, PNG, GIF o WebP de máximo 5MB.");
            }

            // Crear carpeta si no existe
            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // Generar nombre único para el archivo
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            // Guardar archivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

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
        throw new NotImplementedException();
    }

    public bool DeleteImage(string filePath)
    {
        throw new NotImplementedException();
    }

    public bool IsValidImage(IFormFile file)
    {
        throw new NotImplementedException();
    }
}