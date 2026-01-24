namespace Asistencia.Models.ViewModels;
public class CourseAttentionViewModel
    {
        // --- 1. CONTEXTO GENERAL ---
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        // --- 2. DATOS PARA EL FORMULARIO ---
        // Lista de alumnos matriculados en este curso (para llenar los selectores)
        public List<StudentSimpleDto> EnrolledStudents { get; set; } = new List<StudentSimpleDto>();

        // --- 3. ESTADÍSTICAS DEL DASHBOARD (ENCABEZADO) ---
        public int TotalIncidents { get; set; }
        public int AcademicCount { get; set; }
        public int BehavioralCount { get; set; } // Conducta
        public int PendingCount { get; set; }    // Pendientes de resolver

        // --- 4. HISTORIAL DE REGISTROS (FEED) ---
        public List<AttentionRecordDto> History { get; set; } = new List<AttentionRecordDto>();
    }