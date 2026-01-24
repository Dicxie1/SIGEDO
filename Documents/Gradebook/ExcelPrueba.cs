using ClosedXML.Excel;
using Asistencia.Models;
using Asistencia.Services.Interfaces;
namespace Asistencia.Documents;
class ExcelPrueba : IGradebookExportService
{
    public byte[]  GenerateExportReport(Course course, List<AcademicTerm> terms, List<Enrollment> enrollments)
    {
         using (var workbook = new XLWorkbook())
        {
            var ws = workbook.Worksheets.Add("Sábana de Notas");

            var encabezado = ws.Range("A2:A5");
            encabezado.Style.Alignment.TextRotation = 90;
            ws.Merge().Value ="Trabajo Sistema Red";
            ws.Row(2).Height = 10;
            ws.Column(2).Width = 20;
            
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
        
    }
}