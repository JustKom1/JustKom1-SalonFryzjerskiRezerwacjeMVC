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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                ServicesCount = await _context.Services.AsNoTracking().CountAsync(),
                SlotsCount = await _context.Slots.AsNoTracking().CountAsync(),

                PendingCount = await _context.Reservations.AsNoTracking().CountAsync(r => r.Status == "Pending"),
                ApprovedCount = await _context.Reservations.AsNoTracking().CountAsync(r => r.Status == "Approved"),
                RejectedCount = await _context.Reservations.AsNoTracking().CountAsync(r => r.Status == "Rejected"),
                CancelledCount = await _context.Reservations.AsNoTracking().CountAsync(r => r.Status == "Cancelled"),
            };

            // pobierz ostatnie Pending (bez Include(User))
            var pending = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Slot).ThenInclude(s => s.Service)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.Id)
                .Take(10)
                .ToListAsync();

            // mapowanie userId -> email
            var userIds = pending.Select(r => r.UserId).Where(x => x != null).Distinct().ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            var emailById = users.ToDictionary(x => x.Id, x => x.Email);

            vm.LatestPending = pending.Select(r => new AdminPendingRowVm
            {
                Id = r.Id,
                Status = r.Status,
                Date = r.Slot?.Date,
                ServiceName = r.Slot?.Service?.Name,
                UserEmail = (r.UserId != null && emailById.ContainsKey(r.UserId)) ? emailById[r.UserId] : r.UserId
            }).ToList();

            return View(vm);
        }
    }
}
