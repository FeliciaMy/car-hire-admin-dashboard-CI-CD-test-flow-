using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    /// <summary>
  
   
    /// </summary>
    public class JobApplicationViewModel
    {
        public int ApplicationId { get; set; }
        public int VacancyId { get; set; }
        public string VacancyName { get; set; }
        public int UserId { get; set; }
        public string DriverName { get; set; }
        public string LicenseNumber { get; set; }
        public string? ResumePath { get; set; }
        public string Status { get; set; }
        public DateTime ApplicationDate { get; set; }
    }
}