using Microsoft.EntityFrameworkCore;
using Asistencia.Models;
using Asistencia.Data.Configurations;
namespace Asistencia.Data;

public class ApplicationDbContext: DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options)
    {
        
    }
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<AttendanceDetail> AttendancesDetails => Set<AttendanceDetail>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Career> Careers => Set<Career>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Student> Students  => Set<Student>();
    public DbSet<Classroom> Classrooms  => Set<Classroom>();
    public DbSet<Subject> Subjects  => Set<Subject>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<AcademicTerm> AcademicTerms => Set<AcademicTerm>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<StudentGrade> StudentGrades => Set<StudentGrade>();
    public DbSet<AttentionRecord> AttentionRecords => Set<AttentionRecord>();
    public DbSet<AttentionParticipant> AttentionParticipants => Set<AttentionParticipant>();
    public DbSet<SyllabusItem> SyllabusItems => Set<SyllabusItem>();
    public DbSet<AcademicPeriod> AcademicPeriods => Set<AcademicPeriod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.IdCourse, e.Status })
            .HasFilter($"\"Status\" = {(int)EnrollmentStatus.Active}")
                .IsUnique();

            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.IdCourse)
                .HasPrincipalKey(c => c.IdCourse);
        });
        // 1. Llave compuesta para Notas (Un estudiante solo tiene una nota por tarea)
        modelBuilder.Entity<StudentGrade>()
            .HasIndex(g => new { g.EnrollmentId, g.AssignmentId }).IsUnique();
        // 2. Relación Bitácora -> Participantes
        modelBuilder.Entity<AttentionParticipant>()
            .HasOne(ap => ap.AttentionRecord)
            .WithMany(ar => ar.Participants)
            .HasForeignKey(ap => ap.AttentionRecordId)
            .OnDelete(DeleteBehavior.Cascade); // Si borro el reporte, borro los participantes
        // 2. Relación Bitácora -> Participantes
        modelBuilder.Entity<AttentionParticipant>()
            .HasOne(ap => ap.AttentionRecord)
            .WithMany(ar => ar.Participants)
            .HasForeignKey(ap => ap.AttentionRecordId)
            .OnDelete(DeleteBehavior.Cascade); // Si borro el reporte, borro los participantes

        // 3. Relación Participante -> Matrícula (Enrollment)
        modelBuilder.Entity<AttentionParticipant>()
            .HasOne(ap => ap.Enrollment)
            .WithMany(e => e.AttentionParticipants)
            .HasForeignKey(ap => ap.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict); // ¡IMPORTANTE! No borrar matrícula si borro un reporte

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasOne(s => s.Course)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.IdCourse)
                .HasPrincipalKey(c => c.IdCourse);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId);
            entity.HasIndex(e => new { e.IdCourse, e.Date })
             .IsUnique()
             .HasDatabaseName("uq_course_attendance_date");
            entity.Property(e => e.Date)
                .IsRequired();

            // Configure the foreign key to use the correct column name
            entity.HasOne(a => a.Course)
                .WithMany(c => c.Attendances)
                .HasForeignKey(a => a.IdCourse)
                .HasPrincipalKey(c => c.IdCourse);
        });
        modelBuilder.Entity<AttendanceDetail>()
            .HasIndex(d => new { d.AttendanceId, d.EnrollmentId })
            .IsUnique();
        // add schedule configuration 
        modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
    }
  
}