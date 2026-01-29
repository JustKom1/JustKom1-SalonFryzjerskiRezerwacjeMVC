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
    public class SlotsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SlotsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin/Slots
        public async Task<IActionResult> Index()
        {
            TempData.Remove("Success");
            TempData.Remove("Error");

            var reservations = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Slot).ThenInclude(s => s.Service)
                .Where(r => r.Slot != null && (r.Status == "Pending" || r.Status == "Approved"))
                .OrderBy(r => r.Slot!.Date)
                .ToListAsync();

            var userIds = reservations
                .Select(r => r.UserId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            var emailById = users.ToDictionary(x => x.Id, x => x.Email);

            var vm = reservations.Select(r => new AdminBookedSlotRowVm
            {
                ReservationId = r.Id,
                Date = r.Slot!.Date,
                ServiceName = r.Slot!.Service?.Name,
                DurationMinutes = r.Slot!.Service?.DurationMinutes ?? 0,
                Status = r.Status,
                UserEmail = (r.UserId != null && emailById.ContainsKey(r.UserId))
                    ? emailById[r.UserId]
                    : r.UserId
            }).ToList();

            return View(vm);
        }
    }
}
