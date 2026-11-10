using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class SchoolSettingsViewModel
    {
        [Display(Name = "Maximum absence percentage (%)")]
        [Range(0, 25, ErrorMessage = "Maximum absence percentage must be between 0 and 25.")]
        public int MaxAbsencePercentage { get; set; }
    }
}