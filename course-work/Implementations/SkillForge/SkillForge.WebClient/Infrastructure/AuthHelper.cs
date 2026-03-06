namespace SkillForge.WebClient.Infrastructure;

public static class AuthHelper
{
    public const string SessionUserNameKey = "SkillForgeUserName";
    public const string SessionIsAdminKey = "SkillForgeIsAdmin";

    public static bool IsAdmin(this HttpContext? context)
    {
        if (context == null) return false;
        var isAdmin = context.Session.GetString(SessionIsAdminKey);
        return string.Equals(isAdmin, "True", StringComparison.OrdinalIgnoreCase);
    }
}
