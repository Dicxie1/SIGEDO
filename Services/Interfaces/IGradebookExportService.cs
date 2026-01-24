using Asistencia.Models;
namespace Asistencia.Services.Interfaces;

public interface IGradebookExportService
{
    byte[] GenerateExportReport(Course course, List<AcademicTerm> terms, List<Enrollment> enrollments);
}