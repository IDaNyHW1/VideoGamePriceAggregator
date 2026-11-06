using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGamePriceAggregator.Models;
using VideoGamePriceAggregator.ViewModels;

namespace VideoGamePriceAggregator.Controllers
{
    public class AccountController : Controller
    {
        private readonly VideoGamePriceAggregatorContext _context;

        public AccountController(VideoGamePriceAggregatorContext context)
        {
            _context = context;
        }

        
        // Регистрация
        

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Games");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string login = model.Login.Trim();
            string email = model.Email.Trim().ToLower();

            bool loginExists = await _context.Users
                .AnyAsync(u => u.Login == login);

            if (loginExists)
            {
                ModelState.AddModelError(
                    nameof(model.Login),
                    "Пользователь с таким логином уже существует.");

                return View(model);
            }

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Пользователь с таким email уже существует.");

                return View(model);
            }

            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                Login = login,
                Email = email,
                PasswordHash = passwordHash,
                RegistrationDate = DateTime.Now,
                IsAdmin = false
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Регистрация успешно завершена. Теперь войдите в аккаунт.";

            return RedirectToAction(nameof(Login));
        }

        
        // Вход
        

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Games");
            }

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string login = model.Login.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(
                    model.Password,
                    user.PasswordHash))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Неверный логин или пароль.");

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Login)
            };

            if (user.IsAdmin)
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, "Admin"));
            }
            else
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, "User"));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                properties);

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Games");
        }

        
        // Выход
        

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Games");
        }
    }
}