using System.Data.SqlTypes;
using Asistencia.Data;
using Asistencia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql;
namespace Asistencia.Controllers;
public class ClassroomController : Controller
{
    private readonly ApplicationDbContext _context;
    public ClassroomController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> AddClassroom([FromBody] Classroom classroom )
    {
        if (ModelState.IsValid)
        {
            try{
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();
            return Json(new {success = true, data = "Datos registrado"});
            }catch(DbUpdateException ex)
            {   
                string message = "Error al guardar los Datos";  
                if(ex.InnerException is PostgresException pgEx)
                {
                    switch (pgEx.SqlState)
                    {
                        case "23505": // llave Primaria duplicada
                            message = $"Ya existe un Registro con este Codigo: {classroom.ClassroomId}";
                            break;
                        case "23502":
                            message ="Faltan datos obligados";
                            break;
                    }
                }
                return Json(new {success = false, message});
            }
        }
        else if(classroom == null)
        {
            return Json(new {success = false, data = "Dato null"});
        }
        return Json(new {success = false, data = "Datos No registrado registrado"});
    }
    public async Task<IActionResult> Index()
    {
        var classrooms =  await _context.Classrooms.ToListAsync();
        return View(classrooms);
    }
    [HttpGet]
    public async Task<IActionResult> EditClassroom(string classroomId)
    {
        if(classroomId == null) return NotFound();
        var classroom = await _context.Classrooms
            .FindAsync(classroomId);
        if(classroom == null) return NotFound();
        return PartialView("_EditClassroom", classroom);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([Bind("")] Classroom classroom)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Classrooms.Update(classroom);
                await _context.SaveChangesAsync();
                return Json(new {success = true, message ="Aula Actualizado"});
            }catch(Exception ex)
            {
                return StatusCode(500, $"Error Interno al intentar guarda en la base de datos ${ex.Message}");
            }
        }
        return Json(new {success= false, message = "Datos Invalido. Verifique los campos"});
    }
    public async Task<IActionResult> Schedule()
    {
        return View();
    }
    public async Task<JsonResult> IsClassroomAvaliable([FromBody]Schedule schedule)
    {
        return Json(new {success = false, message = "Caracteristica en Construcción"});
    }
}