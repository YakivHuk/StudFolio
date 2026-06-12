using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudFolio.Data;
using StudFolio.Filters;
using StudFolio.Models;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudFolio.Controllers
{
    [AuthorizeOrNotFound]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SettingsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<User?> GetAuthenticatedUserAsync()
        {
            string? userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return null;
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null) return NotFound();

            var model = BuildAccountViewModel(user);
            return View(model);
        }

        public async Task<IActionResult> GetAccount()
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null) return NotFound();

            var model = BuildAccountViewModel(user);
            return PartialView("_Account", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(AccountSettingsViewModel model)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.AvatarUrl = string.IsNullOrEmpty(user.Avatar) ? "/images/default-avatar.png" : user.Avatar;
                return PartialView("_Account", model);
            }

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.AvatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(fileStream);
                }

                if (!string.IsNullOrEmpty(user.Avatar) && !user.Avatar.Contains("default-avatar.png"))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.Avatar.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.Avatar = "/uploads/avatars/" + uniqueFileName;
            }

            user.Lastname = model.LastName;
            user.Firstname = model.Name;
            user.Middlename = model.MiddleName;
            user.EducationInstitution = model.Institution;
            user.Specialty = model.Specialty;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, $"{user.Firstname} {user.Lastname}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("AvatarPath", user.Avatar ?? "images/profile.svg")
            };
            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimTypes.Name, ClaimTypes.Role);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

            var updatedModel = BuildAccountViewModel(user);
            updatedModel.StatusMessage = "Особисті дані успішно оновлено.";
            return PartialView("_Account", updatedModel);
        }

        public IActionResult GetSecurity()
        {
            return PartialView("_Security", new SecuritySettingsViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(SecuritySettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_Security", model);
            }

            var user = await GetAuthenticatedUserAsync();
            if (user == null) return NotFound();

            var hasher = new PasswordHasher<User>();
            var passwordVerification = hasher.VerifyHashedPassword(user, user.Password, model.OldPassword);

            if (passwordVerification == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("OldPassword", "Неправильний поточний пароль");
                return PartialView("_Security", model);
            }

            user.Password = hasher.HashPassword(user, model.NewPassword);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var successModel = new SecuritySettingsViewModel
            {
                StatusMessage = "Пароль успішно змінено."
            };
            return PartialView("_Security", successModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Json(new { redirectUrl = Url.Action("Index", "Home") });
        }

        // 7. AJAX завантаження вкладки інтерфейсу (ОНОВЛЕНО)
        [HttpGet]
        public async Task<IActionResult> GetPortfolio()
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null) return NotFound();

            var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p => p.User!.Email == user.Email);

            ViewBag.HasPortfolio = portfolio != null; // Записуємо чи існує портфоліо
            bool isVisible = portfolio?.IsVisible ?? false;

            return PartialView("_Portfolio", isVisible);
        }

        // 8. Зміна видимості портфоліо з налаштувань (ОНОВЛЕНО)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePortfolioVisibility(bool isVisible)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null) return NotFound();

            var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p => p.User!.Email == user.Email);
            if (portfolio != null)
            {
                portfolio.IsVisible = isVisible;
                await _context.SaveChangesAsync();
            }

            ViewBag.HasPortfolio = portfolio != null;
            return PartialView("_Portfolio", isVisible);
        }

        // 9. Видалення портфоліо (ЗБЕРЕЖЕНО)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePortfolio()
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null) return NotFound();

            var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p => p.User!.Email == user.Email);
            if (portfolio != null)
            {
                _context.Portfolios.Remove(portfolio);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectUrl = Url.Action("Index", "Home") });
        }

        private AccountSettingsViewModel BuildAccountViewModel(User user)
        {
            return new AccountSettingsViewModel
            {
                LastName = user.Lastname,
                Name = user.Firstname,
                MiddleName = user.Middlename,
                Institution = user.EducationInstitution,
                Specialty = user.Specialty,
                AvatarUrl = string.IsNullOrEmpty(user.Avatar) ? "/images/profile.svg" : user.Avatar
            };
        }
    }
}