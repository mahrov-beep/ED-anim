namespace Game.ServerRunner;

using System.Security.Claims;

public static class AuthExtensions {
    public static bool TryGetUserId(this HttpContext httpContext, out Guid userId) {
        userId = Guid.Empty;

        return httpContext?.User?.Identity is ClaimsIdentity claimsIdentity &&
               claimsIdentity.FindFirst(ClaimTypes.NameIdentifier) is { } claim &&
               Guid.TryParse(claim.Value, out userId);
    }
}