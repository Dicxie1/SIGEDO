using Asistencia.Data;
using Asistencia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http.Extensions;


namespace Asistencia.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel();
        var califcacion = _context.AttendancesDetails
            .Where(e => e.AttendanceId == 2)
            .Select( e => new {e.EnrollmentId } );
        model.courseAvtive =  _context.Courses
            .Count(c =>  c.isActive == true);
        model.studentCount = await _context.Students
            .CountAsync();
        string hostname = Dns.GetHostName();
        var address = Dns.GetHostAddresses(hostname);
        ViewBag.ipadress = address
            .Where(i => i.AddressFamily == AddressFamily.InterNetwork)
            .FirstOrDefault()!.ToString();
        ViewBag.currentUrl = Request.GetEncodedUrl();
        return View(model);
    }
}