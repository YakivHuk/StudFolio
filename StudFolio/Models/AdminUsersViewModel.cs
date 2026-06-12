using StudFolio.Models;
using System.Collections.Generic;

namespace StudFolio.Models
{
    public class AdminUsersViewModel
    {
        public List<User> Users { get; set; } = new();
        public List<string> Institutions { get; set; } = new();
        public List<string> Specialties { get; set; } = new();
        public List<string> SelectedInstitutions { get; set; } = new();
        public List<string> SelectedSpecialties { get; set; } = new();
        public List<string> SelectedTimeFrames { get; set; } = new();
        public string SearchString { get; set; } = string.Empty;
        public string SortOrder { get; set; } = "Recommended";
        public int CurrentPage { get; set; } = 1;
        public bool HasMore { get; set; }
    }
}