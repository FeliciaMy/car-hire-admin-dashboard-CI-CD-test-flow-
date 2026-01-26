using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    /// <summary>
    /// Warehouse model representing Takealot distribution centers.
    /// </summary>
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Warehouse name is required")]
        [StringLength(100)]
        [Display(Name = "Warehouse Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        public string Address { get; set; }

        // Navigation Properties
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
    }
}