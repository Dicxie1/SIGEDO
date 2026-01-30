using System.ComponentModel.DataAnnotations;
namespace Asistencia.Models
{
    public enum EnumPeriodStatus {
    Closed = 0,
    Active = 1,
}
    public class AcademicPeriod
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly StartPeriod { get; set; }
        public DateOnly ENdPeriodñ { get; set; }
        public EnumPeriodStatus Status { get; set; }
    }
}
