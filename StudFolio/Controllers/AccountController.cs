using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudFolio.Data;
using StudFolio.Models;
using StudFolio.ViewModels;

namespace StudFolio.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AccountController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            string returnUrl = Request.Headers["Referer"].ToString() ?? "/";

            if (!ModelState.IsValid)
            {
                // Збираємо всі помилки валідації в один рядок
                TempData["RegisterError"] = string.Join("<br/>", ModelState.Values
                    .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["OpenModal"] = "register"; // Сигнал для JS відкрити вікно реєстрації
                return Redirect(returnUrl);
            }

            bool userExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (userExists)
            {
                TempData["RegisterError"] = "Користувач з такою поштою вже зареєстрований";
                TempData["OpenModal"] = "register";
                return Redirect(returnUrl);
            }

            string? avatarPath = null;

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.AvatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(fileStream);
                }

                avatarPath = "/uploads/avatars/" + uniqueFileName;
            }

            var user = new User
            {
                Firstname = model.Firstname,
                Lastname = model.Lastname,
                Middlename = model.Middlename,
                Email = model.Email,
                EducationInstitution = model.EducationInstitution,
                Specialty = model.Specialty,
                Avatar = avatarPath
            };

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await Authenticate(user);

            return Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            string returnUrl = Request.Headers["Referer"].ToString() ?? "/";

            if (!ModelState.IsValid)
            {
                TempData["LoginError"] = "Будь ласка, заповніть усі поля коректно";
                TempData["OpenModal"] = "login";
                return Redirect(returnUrl);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user != null)
            {
                var hasher = new PasswordHasher<User>();
                var result = hasher.VerifyHashedPassword(user, user.Password, model.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    await Authenticate(user);
                    return Redirect(returnUrl);
                }
            }

            TempData["LoginError"] = "Неправильна пошта або пароль";
            TempData["OpenModal"] = "login";
            return Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Name, $"{user.Firstname} {user.Lastname}"),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("AvatarPath", user.Avatar ?? "images/profile.svg")
        };

            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimTypes.Name, ClaimTypes.Role);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}