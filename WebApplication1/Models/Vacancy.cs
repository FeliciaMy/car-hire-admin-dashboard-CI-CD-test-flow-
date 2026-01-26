using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    /// <summary>
    /// Vacancy model representing job openings at warehouses.
    /// Drivers can apply for these vacancies through JobApplications.
    /// </summary>
    public class Vacancy
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vacancy name is required")]
        [StringLength(100)]
        [Display(Name = "Vacancy Title")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [ForeignKey("Warehouse")]
        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        // Navigation Properties
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    }
}