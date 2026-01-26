using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    /// <summary>
    /// DashboardViewModel - Used for admin dashboard
    /// Contains summary statistics for the admin overview
    /// </summary>
    public class DashboardViewModel
    {
        public int TotalWarehouses { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalDrivers { get; set; }
        public int TotalVacancies { get; set; }
        public int PendingApplications { get; set; }
        public int AvailableVehicles { get; set; }
        public List<ActivityLog> RecentActivities { get; set; } = new List<ActivityLog>();
    }
}