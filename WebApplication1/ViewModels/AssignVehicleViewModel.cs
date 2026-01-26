using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.ViewModels
{
    public class AssignVehicleViewModel
    {
        [Required(ErrorMessage = "Please select a driver")]
        [Display(Name = "Select Driver")]
        public int DriverId { get; set; }

        public List<SelectListItem> Drivers { get; set; } = new();

        [Required(ErrorMessage = "Please select a vehicle")]
        [Display(Name = "Select Vehicle")]
        public int VehicleId { get; set; }

        public List<SelectListItem> Vehicles { get; set; } = new();
    }
}
