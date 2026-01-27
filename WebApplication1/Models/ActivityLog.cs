using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    /// <summary>
    /// ActivityLog model for tracking admin actions in the system.

    /// </summary>
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        [Display(Name = "User")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Action type is required")]
        [StringLength(100)]
        [Display(Name = "Action Type")]
        public required string ActionType { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual User User { get; set; } = null!;
    }
}