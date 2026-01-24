namespace Asistencia.Models.ViewModels;
public class ProgrammaticProgressViewModel
{
    // Datos del Informe
    public string CourseName {get; set;} = string.Empty;
    public string TermName {get; set;} = string.Empty;
    public int TermId {get; set;}
    // Matricula y Retención
    public CountStat Initial { get; set; } = new CountStat();
    public CountStat Final { get; set; } = new CountStat();
    // --- CAMBIO: Solo Cantidad para No Examinados ---
    public CountStat NotExamined { get; set; } = new CountStat();
    // (Eliminamos NotExaminedPct)
    public CountStat Approved { get; set; } = new CountStat();
    public PercentStat ApprovedPct { get; set; } = new PercentStat();
    public CountStat Failed { get; set; } = new CountStat();
    public PercentStat FailedPct { get; set; } = new PercentStat();



}
public class DemographicStat
{
    // Totales
    public int TotalQty { get; set; }
    public double TotalPct { get; set; }

    // Hombres
    public int MaleQty { get; set; }
    public double MalePct { get; set; }

    // Mujeres
    public int FemaleQty { get; set; }
    public double FemalePct { get; set; }
}

// Sirve para contar CANTIDADES (Enteros)
    public class CountStat
    {
        public int Male { get; set; }
        public int Female { get; set; }

        // Propiedad calculada: Se suma automáticamente.
        // No necesitas asignarle valor manualmente, solo llenas Male y Female.
        public int Total => Male + Female; 
    }

    // (Opcional) Si también necesitas la de PORCENTAJES (Decimales)
    public class PercentStat
    {
        public double Male { get; set; }
        public double Female { get; set; }
        public double Total { get; set; }
    }