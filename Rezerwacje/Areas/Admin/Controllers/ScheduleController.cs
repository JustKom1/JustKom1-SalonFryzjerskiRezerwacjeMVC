using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rezerwacje.Data;
using Rezerwacje.Models;

namespace Rezerwacje.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Schedule
        public async Task<IActionResult> Index()
        {
            TempData.Remove("Success");
            TempData.Remove("Error");

            var list = await _context.SalonDayOverrides
                .OrderBy(o => o.Date)
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SalonDayOverride model)
        {
            // prosty “upsert”: jedna data = jeden rekord
            var existing = await _context.SalonDayOverrides
                .FirstOrDefaultAsync(x => x.Date.Date == model.Date.Date);

            if (existing == null)
            {
                _context.SalonDayOverrides.Add(model);
            }
            else
            {
                existing.IsClosed = model.IsClosed;
                existing.OpenTime = model.OpenTime;
                existing.CloseTime = model.CloseTime;
                existing.Note = model.Note;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Zapisano wyjątek grafiku.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.SalonDayOverrides.FindAsync(id);
            if (item != null)
            {
                _context.SalonDayOverrides.Remove(item);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Usunięto wyjątek grafiku.";
            return RedirectToAction(nameof(Index));
        }
    }
}
