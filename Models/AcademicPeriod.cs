using System.ComponentModel.DataAnnotations;
namespace Asistencia.Models
{
    public enum EnumPeriodStatus
    {
        Closed = 0,
        Active = 1,
        Planning = 2
    }
    public class AcademicPeriod
    {
        [Key]
        public int AcademicPeriodId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly StartPeriod { get; set; }
        public DateOnly EndPeriod { get; set; }
        public EnumPeriodStatus Status { get; set; }
        public List<Schedule>? Schedules { get; set; } = new List<Schedule>();
    }
}
