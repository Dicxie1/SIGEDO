namespace Asistencia.Controllers;

using System.Threading.Tasks;
using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

public class AcademicTermController: Controller
{
    readonly ApplicationDbContext _context;
    public AcademicTermController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet("/Course/{courseid}/EvalutionConfig")]
    public async Task<IActionResult> EvaluationConfig(int courseid)
    {
        var course = await _context.Courses
            .Include(c => c.Subject)
            .Include(a => a.AcademicTerms)
                .ThenInclude(a => a.Assignments)
            .Where(c => c.IdCourse == courseid).FirstOrDefaultAsync();
        if(course == null) return BadRequest();

        var viewModel = new EvaluationConfigViewModel
        {
            CourseId = courseid,
            CourseCode = course?.Subject?.SubjectId ?? "N/A",
            CourseName = course?.Subject?.SubjetName ?? "N/A",
            Terms  = course?.AcademicTerms?.ToList() ?? new List<Models.AcademicTerm>(),
        };
        return View("_ConfigCourse", viewModel);
    }
    [HttpPost("/Course/{courseid}/SaveEvaluationConfig")]
    public async Task<JsonResult> SaveEvaluationConfig([FromBody] AcademicTerm model)
    {
        if(!ModelState.IsValid) return Json(new {success = false, message = "datos incompletos"});
        try
        {
            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new {success= true, message = "Se ha guardado correctamente el corte/parcial"});
        }catch(DbUpdateException ex)
        {
            return Json(new {success = false, message = $"{ex.ToJson()}"});
        }
       
    }
}