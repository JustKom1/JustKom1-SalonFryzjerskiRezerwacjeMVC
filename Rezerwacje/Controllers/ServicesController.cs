using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rezerwacje.Data;

public class ServicesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ServicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var services = await _context.Services.ToListAsync();
        return View(services);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service == null) return NotFound();

        return View(service);
    }
}
