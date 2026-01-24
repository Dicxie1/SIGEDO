namespace Asistencia.Services.Interfaces;
public interface IGradebookService
{
    Task<GradebookViewModel> GetGradebookAsync(int courseId);
    Task<bool> SaveGradeAsync(int enrollmentId, int assignmentId, double score);
    //Task CreateAssignmentAsync(CreateAssignmentDto dto);
}