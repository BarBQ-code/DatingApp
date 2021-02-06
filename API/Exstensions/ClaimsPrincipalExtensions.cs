using System.Security.Claims;

namespace API.Exstensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user) => user.FindFirst(ClaimTypes.Name)?.Value;

        public static int GetId(this ClaimsPrincipal user) => int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }
}