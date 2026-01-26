using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// DriversController handles driver management and vehicle assignment.
    /// Admins can view all drivers and assign available vehicles to them.
    /// Prevents assigning the same vehicle to multiple drivers (one-to-one relationship).
    /// </summary>
    public class DriversController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DriversController(ApplicationDbContext context)
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

        // GET: Drivers
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var drivers = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Vehicle)
                    .ThenInclude(v => v.Warehouse)
                .ToListAsync();

            return View(drivers);
        }

        // GET: Drivers/AssignVehicle
        public async Task<IActionResult> AssignVehicle()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            // Get drivers without assigned vehicles
            var driversWithoutVehicles = await _context.Drivers
                .Include(d => d.User)
                .Where(d => d.AssignedVehicleId == null)
                .ToListAsync();

            // Get vehicles not assigned to any driver
            var availableVehicles = await _context.Vehicles
                .Include(v => v.Warehouse)
                .Where(v => v.Driver == null)
                .ToListAsync();

            // Build the view model with SelectListItems so the view can bind to Model.Drivers / Model.Vehicles
            var model = new AssignVehicleViewModel
            {
                Drivers = driversWithoutVehicles
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = $"{d.User.FullName} - License: {d.LicenseNumber}"
                    })
                    .ToList(),

                Vehicles = availableVehicles
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.Make} {v.Model} ({v.LicensePlate}) - {v.Warehouse.Name}"
                    })
                    .ToList()
            };

            return View(model);
        }

        // POST: Drivers/AssignVehicle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignVehicle(AssignVehicleViewModel model)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                // Check if vehicle is still available
                var vehicle = await _context.Vehicles
                    .Include(v => v.Driver)
                    .Include(v => v.Warehouse)
                    .FirstOrDefaultAsync(v => v.Id == model.VehicleId);

                if (vehicle == null)
                {
                    TempData["Error"] = "Vehicle not found";
                    return RedirectToAction(nameof(AssignVehicle));
                }

                if (vehicle.Driver != null)
                {
                    TempData["Error"] = "This vehicle is already assigned to another driver";
                    return RedirectToAction(nameof(AssignVehicle));
                }

                // Get the driver
                var driver = await _context.Drivers
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == model.DriverId);

                if (driver == null)
                {
                    TempData["Error"] = "Driver not found";
                    return RedirectToAction(nameof(AssignVehicle));
                }

                if (driver.AssignedVehicleId != null)
                {
                    TempData["Error"] = "This driver already has an assigned vehicle";
                    return RedirectToAction(nameof(AssignVehicle));
                }

                // Assign vehicle to driver
                driver.AssignedVehicleId = vehicle.Id;
                _context.Update(driver);

                // Create notification for the driver
                var notification = new Notification
                {
                    UserId = driver.UserId,
                    Message = $"You have been assigned vehicle: {vehicle.Make} {vehicle.Model} ({vehicle.LicensePlate})",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };
                _context.Notifications.Add(notification);

                // Log the activity
                await LogActivity("Vehicle Assigned", 
                    $"Vehicle {vehicle.Make} {vehicle.Model} ({vehicle.LicensePlate}) assigned to {driver.User.FullName}");

                await _context.SaveChangesAsync();

                TempData["Success"] = "Vehicle assigned successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails - populate the model's SelectListItems
            var driversWithoutVehicles = await _context.Drivers
                .Include(d => d.User)
                .Where(d => d.AssignedVehicleId == null)
                .ToListAsync();

            var availableVehicles = await _context.Vehicles
                .Include(v => v.Warehouse)
                .Where(v => v.Driver == null)
                .ToListAsync();

            model.Drivers = driversWithoutVehicles
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = $"{d.User.FullName} - License: {d.LicenseNumber}"
                })
                .ToList();

            model.Vehicles = availableVehicles
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.Make} {v.Model} ({v.LicensePlate}) - {v.Warehouse.Name}"
                })
                .ToList();

            return View(model);
        }

        // POST: Drivers/UnassignVehicle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignVehicle(int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var driver = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Vehicle)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (driver == null)
                return NotFound();

            if (driver.AssignedVehicleId == null)
            {
                TempData["Error"] = "This driver doesn't have an assigned vehicle";
                return RedirectToAction(nameof(Index));
            }

            var vehicleInfo = $"{driver.Vehicle.Make} {driver.Vehicle.Model} ({driver.Vehicle.LicensePlate})";
            driver.AssignedVehicleId = null;
            _context.Update(driver);

            await LogActivity("Vehicle Unassigned", 
                $"Vehicle {vehicleInfo} unassigned from {driver.User.FullName}");

            await _context.SaveChangesAsync();

            TempData["Success"] = "Vehicle unassigned successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}