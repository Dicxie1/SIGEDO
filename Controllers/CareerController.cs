using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Asistencia.Data;
using Asistencia.Models;
namespace Asistencia.Controllers;

public class CareerController : Controller
{
    private readonly ApplicationDbContext _context;

    public CareerController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([Bind("Code,Name")] Career career)
    {
        if(career == null) return BadRequest();
        Console.WriteLine($"Code : {career.Code}, Name: {career.Name}");
        if (ModelState.IsValid)
        {
            _context.Add(career);
            await _context.SaveChangesAsync();
            TempData["Success"] = "agregado la carrera exitosamente";
            return RedirectToAction("Subject", "Index");
        }
        return View();
    }
}
