using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rezerwacje.Data;
using Rezerwacje.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Rezerwacje.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private const int StepMinutes = 15;
        private const int BufferMinutes = 15;

        public ReservationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var reservations = await _context.Reservations
                .Include(r => r.Slot).ThenInclude(s => s.Service)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(reservations);
        }

        // GET: Reservations/Create?serviceId
        public async Task<IActionResult> Create(int? serviceId)
        {
            await RebuildServiceList(serviceId);

            var vm = new ReservationCreateVm
            {
                ServiceId = serviceId ?? 0,
                Date = DateTime.Today.AddDays(1)
            };

            return View(vm);
        }

        // GET: Reservations/AvailableTimes
        [HttpGet]
        public async Task<IActionResult> AvailableTimes(int serviceId, DateTime date)
        {
            var times = await ComputeAvailableTimes(serviceId, date, excludeReservationId: null);
            return Json(times);
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                await RebuildServiceList(vm.ServiceId);
                return View(vm);
            }

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == vm.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("", "Wybrana usługa nie istnieje.");
                await RebuildServiceList(vm.ServiceId);
                return View(vm);
            }

            if (!TimeSpan.TryParseExact(vm.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out var time))
            {
                ModelState.AddModelError("", "Nieprawidłowa godzina.");
                await RebuildServiceList(vm.ServiceId);
                return View(vm);
            }

            var start = vm.Date.Date.Add(time);

            // przelicza dostępność jeszcze raz
            var available = await ComputeAvailableTimes(vm.ServiceId, vm.Date, excludeReservationId: null);
            if (!available.Contains(vm.StartTime))
            {
                ModelState.AddModelError("", "Wybrany termin nie jest już dostępny. Wybierz inny.");
                await RebuildServiceList(vm.ServiceId);
                return View(vm);
            }

            // Tworzy Slot dopiero przy rezerwacji
            var slot = new Slot
            {
                ServiceId = vm.ServiceId,
                Date = start,
                IsBooked = true
            };

            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();

            var reservation = new Reservation
            {
                SlotId = slot.Id,
                UserId = _userManager.GetUserId(User),
                Status = "Pending"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została zgłoszona (oczekuje na zatwierdzenie).";
            return RedirectToAction(nameof(Index));
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var reservation = await _context.Reservations
                .Include(r => r.Slot)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (reservation.UserId != userId) return Forbid();

            await RebuildServiceList(reservation.Slot.ServiceId);

            var vm = new ReservationEditVm
            {
                Id = reservation.Id,
                ServiceId = reservation.Slot.ServiceId,
                Date = reservation.Slot.Date.Date,
                StartTime = reservation.Slot.Date.ToString("HH:mm")
            };

            return View(vm);
        }

        // GET: Reservations/AvailableTimesForEdit?id&serviceId&date
        [HttpGet]
        public async Task<IActionResult> AvailableTimesForEdit(int id, int serviceId, DateTime date)
        {
            var times = await ComputeAvailableTimes(serviceId, date, excludeReservationId: id);
            return Json(times);
        }

        // POST: Reservations/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReservationEditVm vm)
        {
            if (!ModelState.IsValid)
            {
                await RebuildServiceList(vm.ServiceId);
                return View(vm);
            }

            var reservation = await _context.Reservations
                .Include(r => r.Slot)
                .FirstOrDefaultAsync(r => r.Id == vm.Id);

            if (reservation == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (reservation.UserId != userId) return Forbid();

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == vm.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError("", "Wybrana usługa nie istnieje.");
                await RebuildServiceList(vm.ServiceId);
                return View(vm);
            }

            if (!TimeSpan.TryParseExact(vm.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out var time))
            {
                ModelState.AddModelError("", "Nieprawidłowa godzina.");
                await RebuildServiceList(vm.ServiceId);
                return View(vm);
            }

            var start = vm.Date.Date.Add(time);

            var available = await ComputeAvailableTimes(vm.ServiceId, vm.Date, excludeReservationId: vm.Id);
            if (!available.Contains(vm.StartTime))
            {
                ModelState.AddModelError("", "Wybrany termin nie jest już dostępny. Wybierz inny.");
                await RebuildServiceList(vm.ServiceId);
                return View(vm);
            }

            // Aktualizuje istniejący slot
            reservation.Slot.ServiceId = vm.ServiceId;
            reservation.Slot.Date = start;
            reservation.Slot.IsBooked = true;

            // jeśli było Approved, cofa do Pending po zmianie terminu
            if (reservation.Status == "Approved")
                reservation.Status = "Pending";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została zaktualizowana.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Reservations/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Slot).ThenInclude(s => s.Service)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if (reservation.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Slot)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return RedirectToAction(nameof(Index));

            var userId = _userManager.GetUserId(User);
            if (reservation.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (reservation.Slot != null)
                _context.Slots.Remove(reservation.Slot);

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ---------------- helpers ----------------

        // domyślnie godziny jeśli nie ma wyjątku
        private static readonly TimeSpan DefaultOpen = new TimeSpan(10, 0, 0);
        private static readonly TimeSpan DefaultClose = new TimeSpan(18, 0, 0);

        // Pobiera grafik z bazy: dzień zamknięty / skrócone godziny
        private async Task<(bool IsClosed, TimeSpan Open, TimeSpan Close)> GetOpeningWindow(DateTime date)
        {
            var open = DefaultOpen;
            var close = DefaultClose;

            // zakładam DbSet: SalonDayOverrides
            var ov = await _context.SalonDayOverrides
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Date.Date == date.Date);

            if (ov == null)
                return (false, open, close);

            if (ov.IsClosed)
                return (true, open, close);

            if (ov.OpenTime.HasValue) open = ov.OpenTime.Value;
            if (ov.CloseTime.HasValue) close = ov.CloseTime.Value;

            return (false, open, close);
        }

        private async Task<List<string>> ComputeAvailableTimes(int serviceId, DateTime date, int? excludeReservationId)
        {
            if (serviceId <= 0) return new List<string>();

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId);
            if (service == null) return new List<string>();

            if (date.Date < DateTime.Today) return new List<string>();

            // okno otwarcia z grafiku
            var (isClosed, open, close) = await GetOpeningWindow(date);
            if (isClosed) return new List<string>();

            var dayStart = date.Date + open;
            var dayEnd = date.Date + close;

            var newBlockMinutes = service.DurationMinutes + BufferMinutes;
            if (newBlockMinutes <= 0) return new List<string>();

            // rezerwacje w tym dniu (Pending/Approved)
            var query = _context.Reservations
                .Include(r => r.Slot)
                .Where(r =>
                    r.Slot.Date >= dayStart &&
                    r.Slot.Date < dayEnd &&
                    (r.Status == "Pending" || r.Status == "Approved"));

            if (excludeReservationId.HasValue)
                query = query.Where(r => r.Id != excludeReservationId.Value);

            var busyRaw = await query
                .Select(r => new
                {
                    Start = r.Slot.Date,
                    ServiceId = r.Slot.ServiceId
                })
                .ToListAsync();

            var durations = await _context.Services
                .ToDictionaryAsync(s => s.Id, s => s.DurationMinutes);

            var busy = busyRaw
                .Where(b => durations.ContainsKey(b.ServiceId))
                .Select(b => new
                {
                    Start = b.Start,
                    End = b.Start.AddMinutes(durations[b.ServiceId] + BufferMinutes)
                })
                .ToList();

            var times = new List<string>();

            for (var t = dayStart; t.AddMinutes(newBlockMinutes) <= dayEnd; t = t.AddMinutes(StepMinutes))
            {
                // dla dzisiaj: nie pokazuj godzin z przeszłości
                if (date.Date == DateTime.Today && t <= DateTime.Now) continue;

                var candidateEnd = t.AddMinutes(newBlockMinutes);
                bool overlaps = busy.Any(b => t < b.End && candidateEnd > b.Start);

                if (!overlaps)
                    times.Add(t.ToString("HH:mm"));
            }

            return times;
        }

        private async Task RebuildServiceList(int? selectedServiceId)
        {
            var services = await _context.Services.OrderBy(s => s.Name).ToListAsync();
            ViewBag.ServiceList = new SelectList(services, "Id", "Name", selectedServiceId);
        }
    }
}
