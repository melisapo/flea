using flea_WebProj.Models.Entities;

namespace flea_WebProj.Data.Repositories;

public interface IImageRepository
{
    Task<Image?> GetByIdAsync(int id);
    Task<Image?> GetByProductIdAsync(int productId);
    Task<int> CreateAsync(Address address);
    Task<bool> UpdateAsync(Address address);
    Task<bool> DeleteAsync(int addressId);
}

public class ImageRepository
{
    
}