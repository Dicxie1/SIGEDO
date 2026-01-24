using System.ComponentModel.DataAnnotations;
namespace Asistencia.Models;
public class Classroom
{
    [Key]
    public string? ClassroomId {get; set;}
    [Required]
    public string ClassroomName {get; set;} = string.Empty;
    [Required]
    public int Capacity{get;set;}
    [Required]
    public string Location {get;set;} = string.Empty;
    public ICollection<Schedule> Schedules {get;set;} = new List<Schedule>();
}