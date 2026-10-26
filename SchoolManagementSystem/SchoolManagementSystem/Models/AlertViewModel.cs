using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models
{
    public class AlertViewModel
    {
        [Required]
        [MaxLength(500)]
        [Display(Name = "Message")]
        public string Message { get; set; }
    }
}
