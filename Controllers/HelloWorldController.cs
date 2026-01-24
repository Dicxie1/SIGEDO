using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace Asistencia.Controllers;

public class HelloWorldController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    public string Welcome(string name, int numtimes =1)
    {
        return HtmlEncoder.Default.Encode($"Hello {name}, NumTimes is: {numtimes}");
    }
} 