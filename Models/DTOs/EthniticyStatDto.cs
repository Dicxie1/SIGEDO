namespace Asistencia.Models.ViewModels;
public  class EthnicityStatDto{
    public string Name {get; set;} = string.Empty;
    public int Count {get; set;}
    public decimal Percentage {get; set;}
    public string? ColorClass { get; set; }
}