using System.ComponentModel.DataAnnotations;

namespace Asistencia.Models;

public class Student
{
    [Key]
    public string Id { get; set;} = string.Empty;
    [Required(ErrorMessage = "El campo Nombre es obligatorio")]
    public string? Name{get; set;}
    [Required(ErrorMessage = "El campo Apellidos es obligatorio")]
    public string? LastName{get; set;}
    [Required(ErrorMessage = "El campo Sexo es obligatorio")]
    public Sex Sexo{get; set;}
    public EthnicGroup Ethnic{get; set;}
    public int Cellphone{get; set;}
    [Required(ErrorMessage = "El campo Email es obligatorio")]
    public string Email{get; set;} = string.Empty;   
}

public enum Sex
{
    [Display(Name = "Masculino")] Masculino = 1,
    [Display(Name = "Femenino")] Femenino = 2,
}
public enum EthnicGroup
{
    [Display(Name = "Mestizo")] Mestizo = 1,
    [Display(Name = "Miskito")] Miskito = 2,
    [Display(Name = "Creole")] Creole = 3,
    [Display(Name = "Garifuna")] Garifuna = 4,
    [Display(Name = "Mayangna")] Mayangna = 5,
    [Display(Name = "Mayangna")] Rama = 6, 
}