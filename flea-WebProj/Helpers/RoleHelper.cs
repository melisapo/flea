using flea_WebProj.Models;
using flea_WebProj.Models.Entities;
using flea_WebProj.Models.Enums;

namespace flea_WebProj.Helpers
{
    public static class RoleHelper
    {
        // Verifica si un usuario tiene un rol específico
        public static bool HasRole(this User user, RoleType roleType)
        {
            if (user.Roles == null || !user.Roles.Any())
                return false;

            return user.Roles.Any(r => r.Name.Equals(roleType.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        // Verifica si un usuario tiene cualquiera de los roles especificados
        public static bool HasAnyRole(this User user, params RoleType[] roleTypes)
        {
            if (user.Roles == null || !user.Roles.Any())
                return false;

            var roleNames = roleTypes.Select(r => r.ToString()).ToList();
            return user.Roles.Any(r => roleNames.Contains(r.Name, StringComparer.OrdinalIgnoreCase));
        }

        // Verifica si un usuario es Admin
        public static bool IsAdmin(this User user)
        {
            return user.HasRole(RoleType.Admin);
        }

        // Verifica si un usuario es Moderator
        public static bool IsModerator(this User user)
        {
            return user.HasRole(RoleType.Moderator);
        }

        // Obtiene los nombres de roles como lista de strings
        public static List<string> GetRoleNames(this User user)
        {
            return user.Roles?.Select(r => r.Name).ToList() ?? [];
        }
    }
}