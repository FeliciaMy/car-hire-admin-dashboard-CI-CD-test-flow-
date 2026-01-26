using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    /// <summary>
    
    /// </summary>
    public class Driver
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "License number is required")]
        [StringLength(50)]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; }

        [ForeignKey("Vehicle")]
        [Display(Name = "Assigned Vehicle")]
        public int? AssignedVehicleId { get; set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Vehicle? Vehicle { get; set; }
    }
}