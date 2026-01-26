using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// JobApplicationsController handles viewing and processing driver job applications.
    /// Admins can approve/reject applications and notifications are sent to drivers.
    /// Activity logs are created for all application status changes.
    /// </summary>
    public class JobApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        private async Task LogActivity(string actionType, string description)
        {
            var userId = GetCurrentUserId();
            if (userId > 0)
            {
                var log = new ActivityLog
                {
                    UserId = userId,
                    ActionType = actionType,
                    Description = description,
                    Timestamp = DateTime.Now
                };
                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
        }

        // GET: JobApplications
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var applications = await _context.JobApplications
                .Include(ja => ja.User)
                .Include(ja => ja.Vacancy)
                    .ThenInclude(v => v.Warehouse)
                .OrderByDescending(ja => ja.ApplicationDate)
                .Select(ja => new JobApplicationViewModel
                {
                    ApplicationId = ja.Id,
                    VacancyId = ja.VacancyId,
                    VacancyName = ja.Vacancy.Name,
                    UserId = ja.UserId,
                    DriverName = ja.User.FirstName + " " + ja.User.LastName,
                    LicenseNumber = ja.LicenseNumber,
                    ResumePath = ja.ResumePath,
                    Status = ja.Status,
                    ApplicationDate = ja.ApplicationDate
                })
                .ToListAsync();

            return View(applications);
        }

        // POST: JobApplications/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var application = await _context.JobApplications
                .Include(ja => ja.User)
                .Include(ja => ja.Vacancy)
                .FirstOrDefaultAsync(ja => ja.Id == id);

            if (application == null)
                return NotFound();

            // Validate status
            if (status != "Accepted" && status != "Rejected" && status != "Pending")
            {
                TempData["Error"] = "Invalid status";
                return RedirectToAction(nameof(Index));
            }

            // Update application status
            application.Status = status;
            _context.Update(application);

            // Create notification for the driver
            var notification = new Notification
            {
                UserId = application.UserId,
                Message = $"Your application for '{application.Vacancy.Name}' has been {status.ToLower()}.",
                IsRead = false,
                CreatedDate = DateTime.Now
            };
            _context.Notifications.Add(notification);

            // Log the activity
            await LogActivity("Application Processed", 
                $"Application for {application.User.FullName} - Status changed to {status}");

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Application status updated to {status}";
            return RedirectToAction(nameof(Index));
        }

        // GET: JobApplications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (id == null)
                return NotFound();

            var application = await _context.JobApplications
                .Include(ja => ja.User)
                .Include(ja => ja.Vacancy)
                    .ThenInclude(v => v.Warehouse)
                .FirstOrDefaultAsync(ja => ja.Id == id);

            if (application == null)
                return NotFound();

            var viewModel = new JobApplicationViewModel
            {
                ApplicationId = application.Id,
                VacancyId = application.VacancyId,
                VacancyName = application.Vacancy.Name,
                UserId = application.UserId,
                DriverName = application.User.FullName,
                LicenseNumber = application.LicenseNumber,
                ResumePath = application.ResumePath,
                Status = application.Status,
                ApplicationDate = application.ApplicationDate
            };

            return View(viewModel);
        }
    }
}