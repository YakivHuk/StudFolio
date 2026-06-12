using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudFolio.Data;
using StudFolio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudFolio.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PortfolioController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchString,
            List<string>? selectedSpecialties,
            List<string>? selectedTechs,
            string? timePeriod,
            string? sortBy,
            int page = 1)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                sortBy = "recommended";
            }
            // Базовий запит: на головній сторінці показуємо ТІЛЬКИ видимі картки
            var query = _context.Portfolios.Include(p => p.User).Where(p => p.IsVisible).AsQueryable();

            // --- 1. РОЗУМНИЙ ТЕКСТОВИЙ ПОШУК ---
            if (!string.IsNullOrEmpty(searchString))
            {
                var tokens = searchString.Trim().ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var token in tokens)
                {
                    query = query.Where(p => p.User != null && (
                                         p.User.Firstname.ToLower().Contains(token) ||
                                         p.User.Lastname.ToLower().Contains(token) ||
                                         (p.User.Middlename != null && p.User.Middlename.ToLower().Contains(token)) ||
                                         (p.User.Specialty != null && p.User.Specialty.ToLower().Contains(token))
                    ));
                }
            }

            // --- 2. ФІЛЬТР: СПЕЦІАЛЬНІСТЬ (Групи відсікають результати за принципом AND) ---
            if (selectedSpecialties != null && selectedSpecialties.Any())
            {
                query = query.Where(p => p.User != null && !string.IsNullOrEmpty(p.User.Specialty) && selectedSpecialties.Contains(p.User.Specialty.Trim()));
            }

            // --- 3. ФІЛЬТР: ТЕХНОЛОГІЇ ТА ІНСТРУМЕНТИ ---
            if (selectedTechs != null && selectedTechs.Any())
            {
                query = query.Where(p => _context.Posts
                    .Where(post => post.UserID == p.UserID)
                    .Any(post => post.TechnologiesAndTools.Any(t => selectedTechs.Contains(t)))
                );
            }

            // --- 4. ФІЛЬТР: ЧАС ПУБЛІКАЦІЇ ---
            if (!string.IsNullOrEmpty(timePeriod) && timePeriod != "all")
            {
                var now = DateTime.Now;
                query = timePeriod.ToLower() switch
                {
                    "day" => query.Where(p => p.TimeOfPublication >= now.AddDays(-1)),
                    "week" => query.Where(p => p.TimeOfPublication >= now.AddDays(-7)),
                    "month" => query.Where(p => p.TimeOfPublication >= now.AddMonths(-1)),
                    _ => query
                };
            }

            // --- 5. СОРТУВАННЯ ---
            if (sortBy == "recommended")
            {
                // Отримуємо Email поточного глядача з контексту авторизації
                var viewerEmail = User.FindFirstValue(ClaimTypes.Email);
                StudFolio.Models.User? viewer = null;
                List<string> viewerTechs = new List<string>();

                if (!string.IsNullOrEmpty(viewerEmail))
                {
                    viewer = await _context.Users.FirstOrDefaultAsync(u => u.Email == viewerEmail);
                    if (viewer != null)
                    {
                        // Завантажуємо технології, які використовує сам глядач у своїх постах
                        viewerTechs = await _context.Posts
                            .Where(post => post.UserID == viewer.UserID)
                            .SelectMany(post => post.TechnologiesAndTools)
                            .Distinct()
                            .ToListAsync();
                    }
                }

                if (viewer != null)
                {
                    // Умова: "Рекомендовані" НЕ показують портфоліо глядача глядачеві
                    query = query.Where(p => p.User!.Email != viewerEmail);

                    string viewerSpecialty = viewer.Specialty?.Trim().ToLower() ?? "";

                    query = query
                        // 1) Найвищий пріоритет: Збіг за спеціальністю (1 — так, 0 — ні)
                        .OrderByDescending(p => (!string.IsNullOrEmpty(viewerSpecialty) && p.User!.Specialty!.ToLower() == viewerSpecialty) ? 1 : 0)

                        // 2) Середній пріоритет: Кількість спільних технологій у постах автора портфоліо із глядачем
                        .ThenByDescending(p => _context.Posts
                            .Where(post => post.UserID == p.UserID)
                            .SelectMany(post => post.TechnologiesAndTools)
                            .Count(tech => viewerTechs.Contains(tech)))

                        // 3) Найнижчий пріоритет: Найновіші
                        .ThenByDescending(p => p.PortfolioID);
                }
                else
                {
                    // Якщо глядач неавторизований або не вказав дані — сортуємо як найновіші
                    query = query.OrderByDescending(p => p.PortfolioID);
                }
            }
            else if (sortBy == "newest")
            {
                // Звичайні "Найновіші" показують усіх (включаючи глядача)
                query = query.OrderByDescending(p => p.PortfolioID);
            }
            else if (sortBy == "oldest")
            {
                query = query.OrderBy(p => p.PortfolioID);
            }

            // --- 6. ПАГІНАЦІЯ ---
            int pageSize = 9;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var portfolios = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 1. Динамічний вивід спеціальностей (лише з видимих портфоліо)
            ViewBag.AllSpecialties = await _context.Portfolios
                .Where(p => p.IsVisible)
                .Select(p => p.User != null ? p.User.Specialty : null)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            // 2. Динамічний вивід технологій (лише з постів користувачів, чиї портфоліо є видимими)
            var allTechsFromDb = await _context.Posts
                .Where(post => _context.Portfolios.Any(p => p.UserID == post.UserID && p.IsVisible))
                .Select(post => post.TechnologiesAndTools)
                .ToListAsync();

            ViewBag.AllTechs = allTechsFromDb
                .SelectMany(t => t)
                .Where(t => !string.IsNullOrEmpty(t))
                .Select(t => t.Trim())
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            // Передача параметрів назад у ViewBag для коректної роботи фільтрів в Index.cshtml
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;
            ViewBag.SelectedSpecialties = selectedSpecialties ?? new List<string>();
            ViewBag.SelectedTechs = selectedTechs ?? new List<string>();
            ViewBag.TimePeriod = timePeriod ?? "all";
            ViewBag.SortBy = sortBy;

            return View(portfolios);
        }

        // ПЕРЕГЛЯД ПОРТФОЛІО ЗА АДРЕСОЮ: Portfolio/User?id=id
        [HttpGet]
        [ActionName("User")]
        public async Task<IActionResult> PortfolioUser(int id, int? post)
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PortfolioID == id);

            if (portfolio == null)
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }

            if (post.HasValue)
            {
                var directPost = await _context.Posts
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PostID == post.Value);

                if (directPost != null)
                {
                    ViewBag.DirectPost = directPost;
                }
            }

            return View(portfolio);
        }

        // МЕТОД ДЛЯ СТОРІНКИ USER (AJAX ЗАВАНТАЖЕННЯ МОДАЛКИ БЕЗ ПЕРЕЗАВАНТАЖЕННЯ)
        [HttpGet]
        public async Task<IActionResult> GetPostDetails(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PostID == postId);

            if (post == null)
            {
                return NotFound("Пост не знайдено.");
            }

            return PartialView("_PostDetailsModal", post);
        }
    }
}