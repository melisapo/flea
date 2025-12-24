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

    public static void SetUser(this ISession session, User user)
    {
        session.SetInt32(UserIdKey, user.Id);
        session.SetString(UsernameKey, user.Username);
        session.SetString(UserNameKey, user.Name);

        if (user.Roles == null || user.Roles.Count == 0) return;
        
        var roleNames =  user.Roles.Select(r => r.Name).ToList();
        session.SetString(RolesKey, JsonSerializer.Serialize(roleNames));
    }

    public static int? GetUserId(this ISession session)
        => session.GetInt32(UserIdKey);

    public static string? GetUsername(this ISession session)
        => session.GetString(UsernameKey);
    
    public static string? GetUserName(this ISession session) 
        => session.GetString(UserNameKey);

    private static List<string> GetUserRoles(this ISession session)
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
    
    public static bool IsAuthenticated(this ISession session)
        => session.GetInt32(UserIdKey).HasValue;

    private static bool HasRole(this ISession session, RoleType roleType)
    {
        var roles = session.GetUserRoles();
        return roles.Contains(roleType.ToString(), StringComparer.OrdinalIgnoreCase);
    }
    
    public static bool HasAnyRole(this ISession session, params RoleType[] roleTypes)
    {
        var userRoles = session.GetUserRoles();
        var requiredRoles = roleTypes.Select(r => r.ToString()).ToList();
            
        return userRoles.Any(r => requiredRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
    }

    public static bool IsAdmin(this ISession session)
        => session.HasRole(RoleType.Admin);

    public static bool IsModerator(this ISession session)
        => session.HasRole(RoleType.Moderator);

    public static void ClearUser(this ISession session)
    {
        session.Remove(UserIdKey);
        session.Remove(UsernameKey);
        session.Remove(UserNameKey);
        session.Remove(RolesKey);
    }

    public static void Logout(this ISession session)
        => session.Clear();
}