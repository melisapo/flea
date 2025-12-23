using flea_WebProj.Models.Enums;
using flea_WebProj.Models;
using flea_WebProj.Models.Entities;

namespace flea_WebProj.Helpers
{
    // Clase static con métodos de extensión para facilitar el manejo de productos
    public static class ProductHelper
    {
        // Convierte el status string de la BD al enum
        public static ProductStatus GetStatus(this Product product)
        {
            return Enum.TryParse<ProductStatus>(product.Status, true, out var status) 
                ? status 
                : ProductStatus.Available;
        }

        // Verifica si el producto está disponible
        public static bool IsAvailable(this Product product)
        {
            return product.GetStatus() == ProductStatus.Available;
        }

        // Verifica si el producto está vendido
        public static bool IsSold(this Product product)
        {
            return product.GetStatus() == ProductStatus.Sold;
        }

        // Obtiene el texto en español del estado
        public static string GetStatusText(this Product product)
        {
            return product.GetStatus() switch
            {
                ProductStatus.Available => "Disponible",
                ProductStatus.Sold => "Vendido",
                _ => "Desconocido"
            };
        }

        // Convierte el enum a string para guardar en BD
        public static string ToStatusString(this ProductStatus status)
        {
            return status.ToString();
        }
    }
}