using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class AlertViewModel
    {
        [Required] 
        [MaxLength(100)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Display(Name = "Message")]
        public string Message { get; set; }
    }
}
