using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    /// <summary>
    /// JobApplication model representing driver applications for vacancies.
    /// Links a User (Driver) to a Vacancy and tracks application status.
    /// </summary>
    public class JobApplication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Vacancy")]
        [Display(Name = "Vacancy")]
        public int VacancyId { get; set; }

        [Required]
        [ForeignKey("User")]
        [Display(Name = "Applicant")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "License number is required")]
        [StringLength(50)]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; }

        [StringLength(255)]
        [Display(Name = "Resume Path")]
        public string? ResumePath { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Application Status")]
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

        [Display(Name = "Application Date")]
        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual Vacancy Vacancy { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}