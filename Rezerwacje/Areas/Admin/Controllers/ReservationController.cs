using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rezerwacje.Areas.Admin.Models;
using Rezerwacje.Data;

namespace Rezerwacje.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin/Reservations
        public async Task<IActionResult> Index()
        {
            TempData.Remove("Success");
            TempData.Remove("Error");

            var pending = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Slot).ThenInclude(s => s.Service)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            var userIds = pending
                .Select(r => r.UserId)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            var emailById = users.ToDictionary(x => x.Id, x => x.Email);

            var vm = pending.Select(r => new AdminReservationRowVm
            {
                Id = r.Id,
                Status = r.Status,
                Date = r.Slot?.Date,
                ServiceName = r.Slot?.Service?.Name,
                UserEmail = (r.UserId != null && emailById.ContainsKey(r.UserId)) ? emailById[r.UserId] : r.UserId
            }).ToList();

            return View(vm);
        }

        // POST: /Admin/Reservations/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Slot)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            if (reservation.Status != "Pending")
            {
                TempData["Error"] = "Ta rezerwacja nie jest już oczekująca.";
                return RedirectToAction(nameof(Index));
            }

            reservation.Status = "Approved";

            if (reservation.Slot != null)
                reservation.Slot.IsBooked = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została zatwierdzona.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Reservations/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Slot)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            if (reservation.Status != "Pending")
            {
                TempData["Error"] = "Ta rezerwacja nie jest już oczekująca.";
                return RedirectToAction(nameof(Index));
            }

            reservation.Status = "Rejected";

            if (reservation.Slot != null)
            {
                reservation.Slot.IsBooked = false;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została odrzucona, termin zwolniony.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Slot)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) return NotFound();

            if (reservation.Status != "Pending" && reservation.Status != "Approved")
            {
                TempData["Error"] = "Tę rezerwację nie można już odwołać.";
                return RedirectToAction("Index", "Slots", new { area = "Admin" });
            }

            reservation.Status = "Cancelled";

            if (reservation.Slot != null)
            {
                reservation.Slot.IsBooked = false;

            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Wizyta została odwołana, termin zwolniony.";
            return RedirectToAction("Index", "Slots", new { area = "Admin" });
        }

    }
}
