public class UpdateAttentionRecordDto
{
    public int RecordId { get; set; }
    public int Category { get; set; }
    public int Priority { get; set; }
    public int Status { get; set; } // 1=Pending, 2=Resolved
    public string Observation { get; set; }
}