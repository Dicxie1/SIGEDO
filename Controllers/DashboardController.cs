using Asistencia.Data;
using Asistencia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http.Extensions;
using Asistencia.Services;


namespace Asistencia.Controllers;

public class DashboardController : Controller
{
    private readonly DashboardService _service;
    public DashboardController(DashboardService service)
    {
        _service = service;
    }
    public async Task<IActionResult> Index()
    {
        var model = await _service.GetDashboardViewModelAsync();

        string hostname = Dns.GetHostName();
        var address = Dns.GetHostAddresses(hostname);
        ViewBag.ipadress = address
            .Where(i => i.AddressFamily == AddressFamily.InterNetwork)
            .FirstOrDefault()!.ToString();
        ViewBag.currentUrl = Request.GetEncodedUrl();
        return View(model);
    }
}