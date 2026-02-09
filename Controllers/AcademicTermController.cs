namespace Asistencia.Controllers;
using System;
using System.Threading.Tasks;
using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.DTOs;
using Asistencia.Models.ViewModels;
using Asistencia.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

public class AcademicTermController : Controller
{
    private readonly AcademicTermService _academicTermService;
    public AcademicTermController(AcademicTermService academicTermService)
    {
        _academicTermService = academicTermService;
    }
    [HttpGet("/Course/{courseid}/EvalutionConfig")]
    public async Task<IActionResult> EvaluationConfig(int courseid)
    {
        var viewModel = await _academicTermService.GetCourseTerm(courseid);
        if (viewModel == null) return BadRequest();
        return View("_ConfigCourse", viewModel);
    }
    [HttpPost("/Course/{courseid}/SaveEvaluationConfig")]
    public async Task<JsonResult> SaveEvaluationConfig([FromBody] AcademicTerm model)
    {
        if (!ModelState.IsValid) return Json(new { success = false, message = "datos incompletos" });
        try
        {
            _academicTermService.SaveAcademicTerm(model);
            return Json(new { success = true, message = "Se ha guardado correctamente el corte/parcial" });
        } catch (DbUpdateException ex)
        {
            return Json(new { success = false, message = $"{ex.ToJson()}" });
        }

    }
    /// <summary>
    /// Rest Api get a specific Academic Term
    /// </summary>
    /// <param name="courseId"></param>
    /// <param name="termId"></param>
    /// <returns></returns>
    [HttpPost("/Course/{courseId}/GetEvalutionTerm")]
    public async Task<JsonResult> GetEvaluationTerm(int courseId, [FromBody] TermDto model)
    {
        System.Console.WriteLine($"el termId : {model.CourseId}");
        System.Console.WriteLine($"courseId :  {courseId}");
        if (ModelState.IsValid)
        {
            var term = await _academicTermService.GetAcademcTermAsync(courseId, model.TermId);
            return Json(new { success = true, data = term });
        }
        else
        {
            return Json(new { success = false, message = "BadReques" });
        }

    }
    [HttpPost("/Course/{courseid}/UpdateEvaluationTerm")]
    public async Task<JsonResult> UpdateEvaluationTerm([FromBody] AcademicTerm model)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return Json(new { success = false, message = "Datos inválidos: " + errors });
        }
        var result = await _academicTermService.UpdateAcademicTerm(model);
        return Json(new { success = true, message = result });
    }
}