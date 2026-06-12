using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudFolio.Models
{
    public class AccountSettingsViewModel
    {
        public string? AvatarUrl { get; set; }

        public IFormFile? AvatarFile { get; set; }

        [Required(ErrorMessage = "Будь ласка, введіть прізвище")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Будь ласка, введіть ім'я")]
        public string Name { get; set; } = string.Empty;

        public string? MiddleName { get; set; }

        public string? Institution { get; set; }

        public string? Specialty { get; set; }

        public string? StatusMessage { get; set; }

        // Блок аналітики
        public int TotalUsers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalPortfolios { get; set; }
        public int TotalUsersWithPortfolio { get; set; }

        // За останній день
        public int NewUsersDay { get; set; }
        public int NewPostsDay { get; set; }
        public int NewPortfoliosDay { get; set; }
        public int NewUsersWithPortfolioDay { get; set; }

        // За останній тиждень
        public int NewUsersWeek { get; set; }
        public int NewPostsWeek { get; set; }
        public int NewPortfoliosWeek { get; set; }
        public int NewUsersWithPortfolioWeek { get; set; }

        // За останній місяць
        public int NewUsersMonth { get; set; }
        public int NewPostsMonth { get; set; }
        public int NewPortfoliosMonth { get; set; }
        public int NewUsersWithPortfolioMonth { get; set; }
    }
}