using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudFolio.Data;
using StudFolio.Models;

namespace StudFolio.Controllers
{
    [Authorize]
    public class MyPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MyPostsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /MyPosts
        public async Task<IActionResult> Index(
            string? searchString,
            List<string>? selectedTypes,
            List<string>? selectedTechs,
            string? timePeriod,
            string? sortBy,
            int page = 1)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // 1. Спочатку витягуємо унікальні фільтри з БД, що належать саме цьому користувачу
            var baseUserPosts = await _context.Posts.Where(p => p.UserID == userId).ToListAsync();

            var availableTypes = baseUserPosts.Select(p => p.Type).Where(t => !string.IsNullOrEmpty(t)).Distinct().OrderBy(t => t).ToList();
            var availableTechs = baseUserPosts.SelectMany(p => p.TechnologiesAndTools).Where(t => !string.IsNullOrEmpty(t)).Distinct().OrderBy(t => t).ToList();

            ViewBag.AvailableTypes = availableTypes;
            ViewBag.AvailableTechs = availableTechs;

            // 2. Будуємо запит для виведення відфільтрованих постів
            var query = _context.Posts.Include(p => p.User).Where(p => p.UserID == userId);

            // ПОШУК: за заголовком на збіг
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Title.Contains(searchString.Trim()));
            }

            // ФІЛЬТР: якщо обрано хоча б один чекбокс — фільтруємо, інакше (якщо порожньо) — виводимо всі
            if (selectedTypes != null && selectedTypes.Any())
            {
                query = query.Where(p => selectedTypes.Contains(p.Type));
            }

            if (selectedTechs != null && selectedTechs.Any())
            {
                query = query.Where(p => p.TechnologiesAndTools.Any(t => selectedTechs.Contains(t)));
            }

            // ФІЛЬТР ЧАСУ: "all" (За весь час) є значенням за замовчуванням
            if (!string.IsNullOrEmpty(timePeriod) && timePeriod != "all")
            {
                var now = DateTime.Now;
                if (timePeriod == "day")
                {
                    query = query.Where(p => p.TimeOfPublication >= now.AddDays(-1));
                }
                else if (timePeriod == "week")
                {
                    query = query.Where(p => p.TimeOfPublication >= now.AddDays(-7));
                }
                else if (timePeriod == "month")
                {
                    query = query.Where(p => p.TimeOfPublication >= now.AddMonths(-1));
                }
            }

            // СОРТУВАННЯ
            if (string.IsNullOrEmpty(sortBy) || sortBy == "newest")
            {
                query = query.OrderByDescending(p => p.TimeOfPublication); // За замовчуванням найновіші
            }
            else if (sortBy == "oldest")
            {
                query = query.OrderBy(p => p.TimeOfPublication);
            }
            else if (sortBy == "techCount")
            {
                query = query.OrderByDescending(p => p.TechnologiesAndTools.Count); // За кількістю технологій
            }

            // ПАГІНАЦІЯ (11 карток на сторінці)
            int pageSize = 11;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var posts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Передаємо стан назад у View, щоб зберегти вибір користувача в інтерфейсі
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;
            ViewBag.SelectedTypes = selectedTypes ?? new List<string>();
            ViewBag.SelectedTechs = selectedTechs ?? new List<string>();
            ViewBag.TimePeriod = timePeriod ?? "all";
            ViewBag.SortBy = sortBy ?? "newest";

            return View(posts);
        }

        // POST: /MyPosts/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int postID, string title, string description, string type,
            List<string>? technologiesAndTools, List<string>? links, List<string>? embeddedLinks, IFormFile? previewInput)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            string previewPath = "/images/default-post-preview.jpg";

            // Завантаження нового фото обкладинки, якщо передано файл
            if (previewInput != null && previewInput.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "previews");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(previewInput.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await previewInput.CopyToAsync(fileStream);
                }
                previewPath = "/uploads/previews/" + uniqueFileName;
            }

            if (postID == 0)
            {
                // Режим: Створення нового посту
                var newPost = new Post
                {
                    Title = title.Trim(),
                    Description = description.Trim(),
                    Type = type,
                    TechnologiesAndTools = technologiesAndTools ?? new List<string>(),
                    Links = links ?? new List<string>(),
                    EmbeddedLinks = embeddedLinks ?? new List<string>(),
                    Preview = previewPath,
                    TimeOfPublication = DateTime.Now,
                    UserID = userId
                };
                _context.Posts.Add(newPost);
            }
            else
            {
                // Режим: Редагування існуючого запису
                var existingPost = await _context.Posts.FirstOrDefaultAsync(p => p.PostID == postID && p.UserID == userId);
                if (existingPost == null) return Forbid();

                existingPost.Title = title.Trim();
                existingPost.Description = description.Trim();
                existingPost.Type = type;
                existingPost.TechnologiesAndTools = technologiesAndTools ?? new List<string>();
                existingPost.Links = links ?? new List<string>();
                existingPost.EmbeddedLinks = embeddedLinks ?? new List<string>();

                // Оновлюємо картинку тільки якщо користувач прикріпив новий файл
                if (previewInput != null && previewInput.Length > 0)
                {
                    existingPost.Preview = previewPath;
                }

                _context.Posts.Update(existingPost);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: /MyPosts/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var postItem = await _context.Posts.FirstOrDefaultAsync(p => p.PostID == id && p.UserID == userId);

            if (postItem == null) return NotFound();

            _context.Posts.Remove(postItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}