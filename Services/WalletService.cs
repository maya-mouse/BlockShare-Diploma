

namespace BlockShare.Services
{
    public class WalletService
    {
        /*  public string GetWalletAddress(ClaimsPrincipal user)
          {
              // Припустимо, що адреса зберігається в клеймі з типом "WalletAddress"
              return user.Claims.FirstOrDefault(c => c.Type == "WalletAddress")?.Value;
          }*/
        private readonly IHttpContextAccessor _httpContextAccessor;
        public WalletService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetWalletAddress()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
                return null;

            // Припустимо, що адреса зберігається в claim з типом "wallet"
            return user.FindFirst("wallet")?.Value;
        }
    }
}
