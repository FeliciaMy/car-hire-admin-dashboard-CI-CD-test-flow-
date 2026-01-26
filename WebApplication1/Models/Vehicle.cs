using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    /// <summary>
    /// Vehicle model representing delivery vehicles.
    /// Belongs to a warehouse and can be assigned to one driver.
    /// LicensePlate must be unique across all vehicles.
    /// </summary>
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Make is required")]
        [StringLength(50)]
        [Display(Name = "Vehicle Make")]
        public string Make { get; set; }

        [Required(ErrorMessage = "Model is required")]
        [StringLength(50)]
        [Display(Name = "Vehicle Model")]
        public string Model { get; set; }

        [Required(ErrorMessage = "License plate is required")]
        [StringLength(20)]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; }

        [StringLength(255)]
        [Display(Name = "Vehicle Image Path")]
        public string? VehicleImagePath { get; set; }

        [Required]
        [ForeignKey("Warehouse")]
        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        [NotMapped]
        [Display(Name = "Vehicle Details")]
        public string VehicleDetails => $"{Make} {Model} ({LicensePlate})";

        // Navigation Properties
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual Driver? Driver { get; set; }
    }
}