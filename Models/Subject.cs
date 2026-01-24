using System.ComponentModel.DataAnnotations;
namespace Asistencia.Models;
public class Subject
{
    [Key]
    public string SubjectId {get; set;} = string.Empty;
    [Required]
    public string SubjetName {get; set;} = string.Empty;
    [Required]
    public int AcademiYear {get; set;}
    [Required]
    public string Semester {get; set;} = string.Empty;
    [Required]
    public int CareerId {get; set;}
    public Career? Career {get; set;} 
    [Required]
    public string Area {get; set;} = string.Empty;
    [Required]
    public int Credits {get; set;} = 4 ;
}