using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// DashboardController displays the admin dashboard with system overview.
    
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var viewModel = new DashboardViewModel
            {
                TotalWarehouses = await _context.Warehouses.CountAsync(),
                TotalVehicles = await _context.Vehicles.CountAsync(),
                TotalDrivers = await _context.Drivers.CountAsync(),
                TotalVacancies = await _context.Vacancies.CountAsync(),
                PendingApplications = await _context.JobApplications
                    .Where(ja => ja.Status == "Pending")
                    .CountAsync(),
                AvailableVehicles = await _context.Vehicles
                    .Where(v => v.Driver == null)
                    .CountAsync(),
                RecentActivities = await _context.ActivityLogs
                    .Include(al => al.User)
                    .OrderByDescending(al => al.Timestamp)
                    .Take(10)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}