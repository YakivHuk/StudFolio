using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudFolio.Data;
using StudFolio.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StudFolio.Controllers
{
    [Authorize] // Гарантує, що неавторизований користувач сюди не потрапить
    public class MyPortfolioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MyPortfolioController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // 1. AJAX Пошук постів поточного авторизованого користувача за його UserID
        [HttpGet]
        public async Task<IActionResult> SearchPosts(string query)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new List<object>()); // Якщо ID не знайдено в куках/сесії
            }

            var postsQuery = _context.Posts.Where(p => p.UserID == userId);

            if (!string.IsNullOrWhiteSpace(query))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(query));
            }

            var posts = await postsQuery
                .Select(p => new
                {
                    postID = p.PostID,
                    title = p.Title,
                    preview = p.Preview,
                    type = p.Type
                })
                .ToListAsync();

            return Json(posts);
        }

        // 2. Часткове представлення для модалки деталей поста
        [HttpGet]
        public IActionResult GetPostModalPartial(int id)
        {
            var post = _context.Posts
                               .Include(p => p.User)
                               .FirstOrDefault(p => p.PostID == id);

            if (post == null) return NotFound();

            return PartialView("_PostDetailsModal", post);
        }

        // 3. Завантаження даних профілю для преамбули конструктора
        [HttpGet]
        public async Task<IActionResult> GetUserData()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { name = "Неавторизований", specialty = "", education = "", avatar = "" });
            }

            // Знаходимо користувача за числовим ID
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null)
            {
                return Json(new { name = "Користувача не знайдено", specialty = "", education = "", avatar = "" });
            }

            // Збираємо ПІБ до купи
            string fullName = $"{user.Lastname} {user.Firstname} {user.Middlename}".Trim();

            return Json(new
            {
                name = string.IsNullOrWhiteSpace(fullName) ? "Без імені" : fullName,
                specialty = user.Specialty ?? "",
                education = user.EducationInstitution ?? "",
                avatar = !string.IsNullOrEmpty(user.Avatar) ? user.Avatar : ""
            });
        }

        // 4. Завантаження збереженого портфоліо з БД (якщо є)
        [HttpGet]
        public async Task<IActionResult> GetPortfolio()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { exists = false });
            }

            var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p => p.UserID == userId);

            if (portfolio == null || string.IsNullOrWhiteSpace(portfolio.Content))
            {
                return Json(new { exists = false });
            }

            // ОБОВ'ЯЗКОВО повертаємо первинний ключ id (наприклад, portfolio.PortfolioID або portfolio.Id)
            return Json(new { exists = true, content = portfolio.Content, id = portfolio.PortfolioID });
        }

        // 5. Збереження чистої розмітки конструктора в БД
        [HttpPost]
        public async Task<IActionResult> SavePortfolio(string content)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Користувач не авторизований.");
            }

            var portfolio = await _context.Portfolios.FirstOrDefaultAsync(p => p.UserID == userId);

            if (portfolio == null)
            {
                // Якщо портфоліо ще немає — створюємо новий запис
                portfolio = new Portfolio
                {
                    UserID = userId,
                    Content = content
                };
                _context.Portfolios.Add(portfolio);
            }
            else
            {
                // Якщо є — оновлюємо контент
                portfolio.Content = content;
                _context.Portfolios.Update(portfolio);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}