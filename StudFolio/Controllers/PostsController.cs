using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudFolio.Data;
using StudFolio.Models;

namespace StudFolio.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(
            string? searchString,
            List<string>? selectedTypes,
            List<string>? selectedTechs,
            List<string>? selectedSpecialties,
            string? timePeriod,
            string? sortBy,
            int? post,
            int page = 1)
        {
            // Базовий запит — вибираємо пости ВСІХ студентів
            var query = _context.Posts.Include(p => p.User).AsQueryable();

            // 1. Пошук за заголовком
            if (!string.IsNullOrEmpty(searchString))
            {
                var trimmedSearch = searchString.Trim();
                query = query.Where(p => p.Title.Contains(trimmedSearch));
            }

            // 2. Фільтрація за типом посту
            if (selectedTypes != null && selectedTypes.Any())
            {
                query = query.Where(p => selectedTypes.Contains(p.Type));
            }

            // 3. Фільтрація за спеціальністю автора
            if (selectedSpecialties != null && selectedSpecialties.Any())
            {
                query = query.Where(p => p.User != null && !string.IsNullOrEmpty(p.User.Specialty) && selectedSpecialties.Contains(p.User.Specialty));
            }

            // 4. Фільтрація за технологіями (перевірка перетину списків)
            if (selectedTechs != null && selectedTechs.Any())
            {
                query = query.Where(p => p.TechnologiesAndTools.Any(t => selectedTechs.Contains(t)));
            }

            // 5. Фільтрація за часовим періодом публікації
            if (!string.IsNullOrEmpty(timePeriod))
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

            // 5.5. Отримання профілю глядача для алгоритму "Рекомендовані"
            string? viewerSpecialty = null;
            List<string> viewerTechs = new List<string>();
            int currentUserId = 0; // Запам'ятовуємо ID глядача для подальшого виключення його постів

            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    currentUserId = userId;

                    // Отримуємо спеціальність поточного глядача
                    viewerSpecialty = await _context.Users
                        .Where(u => u.UserID == userId)
                        .Select(u => u.Specialty)
                        .FirstOrDefaultAsync();

                    // Отримуємо список технологій, які використовує глядач (збираємо з його власних постів)
                    viewerTechs = await _context.Posts
                        .Where(p => p.UserID == userId && p.TechnologiesAndTools != null)
                        .SelectMany(p => p.TechnologiesAndTools)
                        .Where(t => !string.IsNullOrEmpty(t))
                        .Select(t => t.Trim())
                        .Distinct()
                        .ToListAsync();
                }
            }

            // 6. Сортування та специфічна фільтрація для "Рекомендовані"
            if (string.IsNullOrEmpty(sortBy) || sortBy.ToLower() == "recommended")
            {
                // КРИТІРІЙ ВИКЛЮЧЕННЯ: Якщо користувач авторизований, прибираємо його пости З РЕКОМЕНДАЦІЙ
                if (currentUserId > 0)
                {
                    query = query.Where(p => p.UserID != currentUserId);
                }

                var trimmedSpecialty = viewerSpecialty?.Trim();

                query = query
                    // Критерій 1 (Найвищий): Збіг за спеціальністю автора та глядача
                    .OrderByDescending(p => p.User != null && !string.IsNullOrEmpty(p.User.Specialty) && !string.IsNullOrEmpty(trimmedSpecialty) && p.User.Specialty.Trim() == trimmedSpecialty ? 1 : 0)

                    // Критерій 2 (Середній): Кількість технологій у пості, що збігаються з технологіями глядача
                    .ThenByDescending(p => p.TechnologiesAndTools.Count(t => viewerTechs.Contains(t)))

                    // Критерій 3 (Найнижчий): Новизна (найновіші пости попереду)
                    .ThenByDescending(p => p.TimeOfPublication);
            }
            else
            {
                // В усіх інших режимах сортування власні пости користувача будуть відображатися
                query = sortBy.ToLower() switch
                {
                    "newest" => query.OrderByDescending(p => p.TimeOfPublication),
                    "oldest" => query.OrderBy(p => p.TimeOfPublication),
                    "title" => query.OrderBy(p => p.Title),
                    "techcount" => query.OrderByDescending(p => p.TechnologiesAndTools.Count),
                    _ => query.OrderByDescending(p => p.TimeOfPublication)
                };
            }

            // 7. Пагінація
            int pageSize = 12;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var posts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            if (post.HasValue)
            {
                ViewBag.TargetPostId = post.Value; // Передаємо ID для JS

                if (!posts.Any(p => p.PostID == post.Value))
                {
                    // Завантажуємо цей пост окремо з бази, щоб відрендерити його модалку
                    var directPost = await _context.Posts
                        .Include(p => p.User)
                        .FirstOrDefaultAsync(p => p.PostID == post.Value);

                    if (directPost != null)
                    {
                        ViewBag.DirectPost = directPost;
                    }
                }
            }
            // 8. Збір унікальних даних з БД для динамічного заповнення списків у фільтрах
            ViewBag.AllSpecialties = await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Specialty))
                .Select(u => u.Specialty!.Trim())
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            ViewBag.AllTypes = await _context.Posts
                .Where(p => !string.IsNullOrEmpty(p.Type))
                .Select(p => p.Type.Trim())
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            var allTechsFromDb = await _context.Posts
                .Where(p => p.TechnologiesAndTools != null)
                .Select(p => p.TechnologiesAndTools)
                .ToListAsync();

            ViewBag.AllTechs = allTechsFromDb
                .SelectMany(t => t)
                .Where(t => !string.IsNullOrEmpty(t))
                .Select(t => t.Trim())
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            // Збереження стану для UI компонентів представлення
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;
            ViewBag.SelectedTypes = selectedTypes ?? new List<string>();
            ViewBag.SelectedTechs = selectedTechs ?? new List<string>();
            ViewBag.SelectedSpecialties = selectedSpecialties ?? new List<string>();
            ViewBag.TimePeriod = timePeriod;
            ViewBag.SortBy = sortBy ?? "recommended";

            return View(posts);
        }
    }
}