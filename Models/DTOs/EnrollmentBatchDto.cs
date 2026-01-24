public class EnrollmentBatchDto
{
    public int CourseId { get; set; }
    public List<string> Students { get; set; } = new List<string>();
}