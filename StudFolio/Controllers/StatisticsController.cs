using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudFolio.Data;
using StudFolio.Filters;
using StudFolio.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudFolio.Controllers
{
    [AuthorizeOrNotFound("owner")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = await GetAnalyticsViewModelAsync();
            return View(model);
        }

        private async Task<AccountSettingsViewModel> GetAnalyticsViewModelAsync()
        {
            var now = DateTime.Now;
            var oneDayAgo = now.AddDays(-1);
            var oneWeekAgo = now.AddDays(-7);
            var oneMonthAgo = now.AddMonths(-1);

            // Будуємо аналітику на базі вашої моделі
            var model = new AccountSettingsViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalPosts = await _context.Posts.CountAsync(),
                TotalPortfolios = await _context.Portfolios.CountAsync(),
                TotalUsersWithPortfolio = await _context.Users.CountAsync(u => u.Portfolio != null),

                NewUsersDay = await _context.Users.CountAsync(u => u.CreationTime >= oneDayAgo),
                NewPostsDay = await _context.Posts.CountAsync(p => p.TimeOfPublication >= oneDayAgo),
                NewPortfoliosDay = await _context.Portfolios.CountAsync(p => p.TimeOfPublication >= oneDayAgo),
                NewUsersWithPortfolioDay = await _context.Users.CountAsync(u => u.Portfolio != null && u.CreationTime >= oneDayAgo),

                NewUsersWeek = await _context.Users.CountAsync(u => u.CreationTime >= oneWeekAgo),
                NewPostsWeek = await _context.Posts.CountAsync(p => p.TimeOfPublication >= oneWeekAgo),
                NewPortfoliosWeek = await _context.Portfolios.CountAsync(p => p.TimeOfPublication >= oneWeekAgo),
                NewUsersWithPortfolioWeek = await _context.Users.CountAsync(u => u.Portfolio != null && u.CreationTime >= oneWeekAgo),

                NewUsersMonth = await _context.Users.CountAsync(u => u.CreationTime >= oneMonthAgo),
                NewPostsMonth = await _context.Posts.CountAsync(p => p.TimeOfPublication >= oneMonthAgo),
                NewPortfoliosMonth = await _context.Portfolios.CountAsync(p => p.TimeOfPublication >= oneMonthAgo),
                NewUsersWithPortfolioMonth = await _context.Users.CountAsync(u => u.Portfolio != null && u.CreationTime >= oneMonthAgo)
            };

            return model;
        }
    }
}