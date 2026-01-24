public class SyllabusPreviewRowDto
{
    public int RowNumber { get; set; }
    public bool IsValid { get; set; }
    public string? Error { get; set; }

    public DateOnly Date { get; set; }
    public string Objectives { get; set; } = "";
    public string Content { get; set; } = "";
    public string Strategies { get; set; } = "";
    public string Resources { get; set; } = "";
    public string Evaluations { get; set; } = "";
    public string Bibliography { get; set; } = "";
    public string DateFormatted { get; set; } = "";
}