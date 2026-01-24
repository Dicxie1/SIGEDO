using Microsoft.AspNetCore.Mvc;
using Asistencia.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Asistencia.Models;
namespace Asistencia.Controllers;
public class SubjectController : Controller
{
    private readonly ApplicationDbContext _context;

    public SubjectController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IActionResult> Index()
    {
        var careers = await _context.Careers
            .Include( c => c.Subjects)
            .ToListAsync();
        ViewBag.CareerList = new SelectList(careers, "CareerId", "Name");
        return View(careers);
    }
    public async Task<IActionResult> Admin()
    {
        var careers = await _context.Careers
            .Include( c => c.Subjects)
            .ToListAsync();
        ViewBag.CareerList = new SelectList(careers, "Id", "Name");
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSubject( [FromBody] Subject subject)
    {
        if (ModelState.IsValid)
        {
            _context.Add(subject);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Asignatura Registrado"});
        }
        return Json(new { success = false, message = $"Datos Incorrecto: SubjectId: {subject.SubjectId} SubjetName: {subject.SubjetName} " +
                                                    $"Semester: {subject.Semester} Academi: {subject.AcademiYear} Credists: {subject.Credits} " +
                                                    $"Career: {subject.CareerId} Area: {subject.Area}"});
    } 
    [HttpPost]
    public async Task<IActionResult> AddCareer([FromBody] Career career)
    {
        if(ModelState.IsValid)
        {
            _context.Add(career);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Carrera Registrado "});
        }
        return Json(new { success = false, message = "Datos Incorrecto"});
    } 
    
    public async Task<IActionResult> GetCourses(string subjectId)
    {
        var coursesList =  _context.Courses
            .Where(c => c.SubjectId == subjectId)
            .Include(e => e.Enrollments)
            .Select(c => new
            {
                c.Year,
                students = c.Enrollments.Count(),
                c.isActive

            });
        return Json(new {success = true, courses = coursesList});
    }
    [HttpGet]
    public async Task<JsonResult> GetSubjectByCareer(int careerId)
    {
        var subjects = await _context.Subjects
            .Include(c => c.Career)
            .Where( s =>  s.CareerId == careerId)
            .Select(s => new
            {
                id = s.SubjectId,
                name = s.SubjetName
            })
            .ToListAsync();
        if(subjects == null)
        {
            return Json( new {success = false, msg = "No existe carrera"});
        }
        return Json(new {success = true, data = subjects});
    }
    [HttpGet]
    public  async Task<IActionResult> GetSubjectCredit(string subjectId)
    {
        var subjectCredit = await _context.Subjects
            .Where(s => s.SubjectId == subjectId)
            .Select( e => new { Credit = e.Credits}).FirstOrDefaultAsync();
        if( subjectCredit?.Credit == 0) return Json(new {success = false, data = "N/D"});
        return Json(new {success = true, data = subjectCredit});
    }
}