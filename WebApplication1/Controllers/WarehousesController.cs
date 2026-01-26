using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// WarehousesController handles full CRUD operations for warehouses.
    /// Admins can create, view, edit, and delete warehouse locations.
    /// All operations are logged in ActivityLog.
    /// </summary>
    public class WarehousesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WarehousesController(ApplicationDbContext context)
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

        // GET: Warehouses
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var warehouses = await _context.Warehouses
                .Include(w => w.Vehicles)
                .ToListAsync();

            return View(warehouses);
        }

        // GET: Warehouses/Create
        public IActionResult Create()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            return View();
        }

        // POST: Warehouses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address")] Warehouse warehouse)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                _context.Add(warehouse);
                await _context.SaveChangesAsync();

                await LogActivity("Warehouse Added", 
                    $"New warehouse '{warehouse.Name}' created at {warehouse.Address}");

                TempData["Success"] = "Warehouse created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        // GET: Warehouses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (id == null)
                return NotFound();

            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
                return NotFound();

            return View(warehouse);
        }

        // POST: Warehouses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address")] Warehouse warehouse)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (id != warehouse.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(warehouse);
                    await _context.SaveChangesAsync();

                    await LogActivity("Warehouse Updated", 
                        $"Warehouse '{warehouse.Name}' information updated");

                    TempData["Success"] = "Warehouse updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WarehouseExists(warehouse.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        // GET: Warehouses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (id == null)
                return NotFound();

            var warehouse = await _context.Warehouses
                .Include(w => w.Vehicles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (warehouse == null)
                return NotFound();

            return View(warehouse);
        }

        // POST: Warehouses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse != null)
            {
                _context.Warehouses.Remove(warehouse);
                await _context.SaveChangesAsync();

                await LogActivity("Warehouse Deleted", 
                    $"Warehouse '{warehouse.Name}' has been deleted");

                TempData["Success"] = "Warehouse deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool WarehouseExists(int id)
        {
            return _context.Warehouses.Any(e => e.Id == id);
        }
    }
}