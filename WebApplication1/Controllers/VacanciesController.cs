using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class VacanciesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VacanciesController(ApplicationDbContext context)
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

        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var vacancies = await _context.Vacancies
                .Include(v => v.Warehouse)
                .Include(v => v.JobApplications)
                .ToListAsync();

            return View(vacancies);
        }

        public IActionResult Create()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,WarehouseId")] Vacancy vacancy)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                _context.Add(vacancy);
                await _context.SaveChangesAsync();

                var warehouse = await _context.Warehouses.FindAsync(vacancy.WarehouseId);
                await LogActivity("Vacancy Created", 
                    $"New vacancy '{vacancy.Name}' created at {warehouse?.Name}");

                TempData["Success"] = "Vacancy created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name", vacancy.WarehouseId);
            return View(vacancy);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (id == null)
                return NotFound();

            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy == null)
                return NotFound();

            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name", vacancy.WarehouseId);
            return View(vacancy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,WarehouseId")] Vacancy vacancy)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (id != vacancy.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vacancy);
                    await _context.SaveChangesAsync();

                    await LogActivity("Vacancy Updated", 
                        $"Vacancy '{vacancy.Name}' has been updated");

                    TempData["Success"] = "Vacancy updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VacancyExists(vacancy.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["WarehouseId"] = new SelectList(_context.Warehouses, "Id", "Name", vacancy.WarehouseId);
            return View(vacancy);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (id == null)
                return NotFound();

            var vacancy = await _context.Vacancies
                .Include(v => v.Warehouse)
                .Include(v => v.JobApplications)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vacancy == null)
                return NotFound();

            return View(vacancy);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy != null)
            {
                _context.Vacancies.Remove(vacancy);
                await _context.SaveChangesAsync();

                await LogActivity("Vacancy Deleted", 
                    $"Vacancy '{vacancy.Name}' has been deleted");

                TempData["Success"] = "Vacancy deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VacancyExists(int id)
        {
            return _context.Vacancies.Any(e => e.Id == id);
        }
    }
}