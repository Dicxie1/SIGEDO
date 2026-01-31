using Asistencia.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Asistencia.Data.Configurations
{
    public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
    {
        public void Configure(EntityTypeBuilder<Schedule> builder)
        {
            // prevent delete classroom whith schedule 
            builder.HasOne(x => x.Teacher)
                .WithMany(x => x.Schedules)
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Classroom)
                .WithMany(x => x.Schedules)
                .HasForeignKey(x => x.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);
            // crear a index for search proccess
            builder.HasIndex(x => new { x.TeacherId, x.DayOfWeek })
                .HasDatabaseName("IX_schedule_teacher_day");
            builder.HasIndex(x => new { x.ClassroomId, x.DayOfWeek })
                .HasDatabaseName("IX_schedule_classroom_day");
            //prevent delete on cascade AcademicPeriod
            builder.HasOne(x => x.AcademicPeriod)
                .WithMany(x=> x.Schedules)
                .HasForeignKey(x => x.AcademicPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
