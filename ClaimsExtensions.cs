using System.Security.Claims;

namespace HandyConnect.Api.Common;

public static class ClaimsExtensions
{
    public static int UserId(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue("uid"), out var id) ? id : 0;
}
