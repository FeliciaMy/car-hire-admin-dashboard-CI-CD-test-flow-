using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// VehiclesController handles full CRUD operations for vehicles.
    /// Vehicles are assigned to warehouses and can be assigned to drivers.
    /// LicensePlate uniqueness is enforced at database level.
    /// </summary>
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
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

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var vehicles = await _context.Vehicles
                .Include(v => v.Warehouse)
                .Include(v => v.Driver)
                    .ThenInclude(d => d.User)
                .ToListAsync();

            return View(vehicles);
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name");
            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Make,Model,LicensePlate,VehicleImagePath,WarehouseId")] Vehicle vehicle)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var existingVehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.LicensePlate == vehicle.LicensePlate);

                if (existingVehicle != null)
                {
                    ModelState.AddModelError("LicensePlate", "This license plate is already registered");
                    ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name", vehicle.WarehouseId);
                    return View(vehicle);
                }

                _context.Add(vehicle);
                await _context.SaveChangesAsync();

                await LogActivity("Vehicle Added",
                    $"New vehicle {vehicle.Make} {vehicle.Model} ({vehicle.LicensePlate}) added");

                TempData["Success"] = "Vehicle created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name", vehicle.WarehouseId);
            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (id == null)
                return NotFound();

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
                return NotFound();

            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name", vehicle.WarehouseId);
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Make,Model,LicensePlate,VehicleImagePath,WarehouseId")] Vehicle vehicle)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (id != vehicle.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var duplicatePlate = await _context.Vehicles
                        .FirstOrDefaultAsync(v => v.LicensePlate == vehicle.LicensePlate && v.Id != vehicle.Id);

                    if (duplicatePlate != null)
                    {
                        ModelState.AddModelError("LicensePlate", "This license plate is already in use");
                        ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name", vehicle.WarehouseId);
                        return View(vehicle);
                    }

                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();

                    await LogActivity("Vehicle Updated",
                        $"Vehicle {vehicle.Make} {vehicle.Model} ({vehicle.LicensePlate}) updated");

                    TempData["Success"] = "Vehicle updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name", vehicle.WarehouseId);
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (id == null)
                return NotFound();

            var vehicle = await _context.Vehicles
                .Include(v => v.Warehouse)
                .Include(v => v.Driver)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vehicle == null)
                return NotFound();

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

                await LogActivity("Vehicle Deleted",
                    $"Vehicle {vehicle.Make} {vehicle.Model} ({vehicle.LicensePlate}) deleted");

                TempData["Success"] = "Vehicle deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
}