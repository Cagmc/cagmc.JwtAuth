namespace cagmc.JwtAuth.WebApi.Common.Constants;

public static class Policies
{
    public const string AdminPolicy = "admin-policy";
    public const string ReadOnlyPolicy = "read-only-policy";
    public const string EditorPolicy = "editor-policy";
    
    public const string JwtPolicy = "jwt-policy";
    public const string CookiePolicy = "cookie-policy";
    public const string MultiAuthPolicy = "multi-auth-policy";
}