using System.Text.Json;
using flea_WebProj.Models.Entities;
using flea_WebProj.Models.Enums;

namespace flea_WebProj.Helpers;

public static class SessionHelper
{
    private const string UserIdKey = "UserId";
    private const string UsernameKey = "Username";
    private const string UserNameKey = "UserName";
    private const string RolesKey = "UserRoles";

    extension(ISession session)
    {
        public void SetUser(User user)
        {
            session.SetInt32(UserIdKey, user.Id);
            session.SetString(UsernameKey, user.Username);
            session.SetString(UserNameKey, user.Name);

            if (user.Roles == null || user.Roles.Count == 0) return;
        
            var roleNames =  user.Roles.Select(r => r.Name).ToList();
            session.SetString(RolesKey, JsonSerializer.Serialize(roleNames));
        }

        public int? GetUserId()
            => session.GetInt32(UserIdKey);

        public string? GetUsername()
            => session.GetString(UsernameKey);

        public string? GetUserName() 
            => session.GetString(UserNameKey);

        private List<string> GetUserRoles()
        {
            var rolesJson = session.GetString(RolesKey);
            if (string.IsNullOrEmpty(rolesJson)) return [];
        
            try
            {
                return JsonSerializer.Deserialize<List<string>>(rolesJson) ?? [];
            }
            catch
            {
                return [];
            }
        }

        public bool IsAuthenticated()
            => session.GetInt32(UserIdKey).HasValue;

        private bool HasRole(RoleType roleType)
        {
            var roles = session.GetUserRoles();
            return roles.Contains(roleType.ToString(), StringComparer.OrdinalIgnoreCase);
        }

        public bool HasAnyRole(params RoleType[] roleTypes)
        {
            var userRoles = session.GetUserRoles();
            var requiredRoles = roleTypes.Select(r => r.ToString()).ToList();
            
            return userRoles.Any(r => requiredRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
        }

        public bool IsAdmin()
            => session.HasRole(RoleType.Admin);

        public bool IsModerator()
            => session.HasRole(RoleType.Moderator);

        public void ClearUser()
        {
            session.Remove(UserIdKey);
            session.Remove(UsernameKey);
            session.Remove(UserNameKey);
            session.Remove(RolesKey);
        }

        public void Logout()
            => session.Clear();
    }
}