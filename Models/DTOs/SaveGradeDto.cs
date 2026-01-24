namespace Asistencia.Models.DTOs;
public class SaveGradeDto
{
    public int EnrollmentId { get; set; }
    public int AssignmentId { get; set; }
    public double Score { get; set; }
}