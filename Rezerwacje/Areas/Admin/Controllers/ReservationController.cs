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
            var pending = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Slot).ThenInclude(s => s.Service)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            // pobierz maile dla UserId (bez nawigacji AppUser)
            var userIds = pending.Select(r => r.UserId).Where(x => x != null).Distinct().ToList();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var r = await _context.Reservations.FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();

            r.Status = "Approved";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została zatwierdzona.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var r = await _context.Reservations.FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();

            r.Status = "Rejected";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została odrzucona.";
            return RedirectToAction(nameof(Index));
        }
    }
}
