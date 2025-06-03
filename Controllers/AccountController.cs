using BlockShare.Data;
using BlockShare.Models;
using BlockShare.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nethereum.Signer;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BlockShare.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
       private readonly EmailService _emailService;

        public AccountController(AppDbContext db
            , EmailService emailService
            )
        {
            _db = db;
            _emailService = emailService;
        }
        [HttpGet]
         public async Task<IActionResult> Profile()
        {
            var userId = User.Identity.Name;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        public IActionResult Register() => View();
        public IActionResult WalletConnectError() => View();

        /*   [HttpPost]
           public async Task<IActionResult> Register(string username, string password, string walletAddress)
           {
               if (_db.Users.Any(u => u.Username == username))
               {
                   ModelState.AddModelError("", "Користувач вже існує");
                   return View();
               }

               var hash = HashPassword(password);
               _db.Users.Add(new User
               {
                   Username = username,
                   PasswordHash = hash,
                   WalletAddress = walletAddress
               });

               await _db.SaveChangesAsync();
               return RedirectToAction("Login");
           }*/
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string walletAddress, string email)
        {
            if (walletAddress.IsNullOrEmpty())
            {
                return RedirectToAction("WalletConnectError");
            }
            if (_db.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("", "Користувач вже існує");
                return View();
            }
			var token = Guid.NewGuid().ToString();
			var hash = HashPassword(password);
            var user = new User
            {
                Username = username,
                PasswordHash = hash,
                WalletAddress = walletAddress,
                Email = email,
                EmailConfirmed = false, // додай це поле в модель
                EmailConfirmationToken = token
			};

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Створити лінк підтвердження
            var confirmLink = Url.Action("ConfirmEmail", "Account", new { token }, Request.Scheme);

            await _emailService.SendConfirmationEmail(email, confirmLink);

            ViewBag.Message = "Лист підтвердження надіслано! Перевірте пошту.";
            return View("RegisterConfirmation");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWallet([FromBody] WalletAuthModel model)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized(new { success = false, error = "Користувач не авторизований" });

            var userId = User.Identity.Name;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == userId);

            if (user == null)
                return NotFound(new { success = false, error = "Користувача не знайдено" });

            var nonce = HttpContext.Session.GetString("WalletNonce");
            if (string.IsNullOrEmpty(nonce))
                return BadRequest(new { success = false, error = "Nonce відсутній або застарілий" });
         
            try
            {
                var signerAddress = new EthereumMessageSigner().EncodeUTF8AndEcRecover(nonce, model.Signature);

                if (!string.Equals(signerAddress, model.Address, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { success = false, error = "Підпис не відповідає адресі" });
                }

                user.WalletAddress = model.Address;
                await _db.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }


        [HttpGet]
		public async Task<IActionResult> ConfirmEmail(string token)
		{
			var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
			if (user == null) return NotFound();

			user.EmailConfirmed = true;
			await _db.SaveChangesAsync();
			return View("EmailConfirmed");
		}


		public IActionResult Login() => View();

        /*        [HttpPost]
                public async Task<IActionResult> Login(string email, string password)
                {
                    var hash = HashPassword(password);
                    var user = _db.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hash);

                    if (user == null)
                    {
                        ModelState.AddModelError("", "Невірні облікові дані");
                        return View();
                    }

                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("UserId", user.Id.ToString())
                };

                    var identity = new ClaimsIdentity(claims, "MyCookieAuth");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("MyCookieAuth", principal);

                    return RedirectToAction("Index", "File");
                }*/

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var hash = HashPassword(password);
            var user = _db.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hash);

            if (user == null)
            {
                ModelState.AddModelError("", "Невірні облікові дані");
                return View();
            }

            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError("", "Підтвердіть свій email перед входом");
                return View();
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim("UserId", user.Id.ToString()),
        new Claim("wallet", user.WalletAddress.ToString())
    };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            return RedirectToAction("Index", "File");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public IActionResult WalletSettings() => View();

        [HttpGet]
        public async Task<IActionResult> ConnectWallet()
        {
            var nonce = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("WalletNonce", nonce);
            return Ok(nonce);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyWallet([FromBody] WalletAuthModel model)
        {
            var savedNonce = HttpContext.Session.GetString("WalletNonce");
            if (savedNonce == null)
                return BadRequest("Сесія завершена");

            var signer = new EthereumMessageSigner();
            var recoveredAddress = signer.EncodeUTF8AndEcRecover(savedNonce, model.Signature);

            if (recoveredAddress.ToLower() != model.Address.ToLower())
                return BadRequest("Підпис некоректний");
            var sameAddress = await _db.Users.FirstOrDefaultAsync(u => u.WalletAddress == model.Address);
            if (sameAddress != null)
                return BadRequest("Такий гаманець уже є в системі");
            /*
                        // Знайти користувача по User.Identity.Name
                        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);

                        if (user != null)
                            return BadRequest("Такий користувач є");

                        user.WalletAddress = model.Address;
                        await _db.SaveChangesAsync();*/

            return Ok("Гаманець прив'язано!");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                // Не кажемо, що юзера немає — для безпеки
                ViewBag.Message = "Інструкції надіслано на email (якщо він існує).";
                return View("ForgotPasswordConfirmation");
            }

            var token = GenerateResetToken(email);
            var link = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);
            await _emailService.SendPasswordResetEmail(email, link);

            ViewBag.Message = "Інструкції надіслано на email.";
            return View("ForgotPasswordConfirmation");
        }

        private string GenerateResetToken(string email)
        {
            var expires = DateTime.UtcNow.AddMinutes(30).ToString("o"); // ISO format
            var data = $"{email}|{expires}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
        }
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var result = ParseResetToken(token);
            if (result == null)
                return View("InvalidOrExpiredToken");

            ViewBag.Token = token;
            return View();
        }

        private (string email, DateTime expiry)? ParseResetToken(string token)
        {
            try
            {
                var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decoded.Split('|');
                if (parts.Length != 2) return null;

                var email = parts[0];
                var expiry = DateTime.Parse(parts[1], null, System.Globalization.DateTimeStyles.RoundtripKind);

                if (expiry < DateTime.UtcNow)
                    return null;

                return (email, expiry);
            }
            catch
            {
                return null;
            }
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            var result = ParseResetToken(token);
            if (result == null)
                return View("InvalidOrExpiredToken");

            var user = _db.Users.FirstOrDefault(u => u.Email == result.Value.email);
            if (user == null)
                return NotFound();

            user.PasswordHash = HashPassword(newPassword);
            await _db.SaveChangesAsync();

            return View("PasswordResetSuccess");
        }


    }
}
