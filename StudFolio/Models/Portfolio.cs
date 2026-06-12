using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudFolio.Models
{
    public class Portfolio
    {
        [Key]
        public int PortfolioID { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime TimeOfPublication { get; set; } = DateTime.Now;

        public bool IsVisible { get; set; } = true;

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public User? User { get; set; }
    }
}