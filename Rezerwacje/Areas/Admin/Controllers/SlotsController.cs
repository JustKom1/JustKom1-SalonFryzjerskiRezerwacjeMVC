using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rezerwacje.Data;

namespace Rezerwacje.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SlotsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SlotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /Admin/Slots/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int days = 14)
        {
            var services = await _context.Services.ToListAsync();
            if (!services.Any())
            {
                TempData["Error"] = "Brak usług. Najpierw dodaj usługi.";
                return RedirectToAction("Index");
            }

            var open = new TimeSpan(10, 0, 0);
            var close = new TimeSpan(18, 0, 0);

            var startDate = DateTime.Today;

            foreach (var service in services)
            {
                var duration = service.DurationMinutes;
                if (duration <= 0) continue;

                var block = duration + 15;

                for (int d = 0; d < days; d++)
                {
                    var day = startDate.AddDays(d);

                    // jeśli chcesz pominąć niedziele:
                    // if (day.DayOfWeek == DayOfWeek.Sunday) continue;

                    var current = day.Date + open;

                    while (current.TimeOfDay.Add(TimeSpan.FromMinutes(block)) <= close)
                    {
                        // nie duplikuj tego samego terminu dla tej usługi
                        bool exists = await _context.Slots.AnyAsync(s =>
                            s.ServiceId == service.Id && s.Date == current);

                        if (!exists)
                        {
                            _context.Slots.Add(new Slot
                            {
                                ServiceId = service.Id,
                                Date = current,
                                IsBooked = false
                            });
                        }

                        current = current.AddMinutes(block);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Wygenerowano terminy na {days} dni (10:00–18:00, +15 min przerwy).";
            return RedirectToAction("Index");
        }

        // GET: /Admin/Slots
        public async Task<IActionResult> Index()
        {
            var slots = await _context.Slots
                .Include(s => s.Service)
                .OrderBy(s => s.Date)
                .ToListAsync();

            return View(slots);
        }
    }
}
