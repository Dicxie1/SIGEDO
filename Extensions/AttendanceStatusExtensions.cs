
using Asistencia.Models.Enums;

namespace Asistencia.Extensions
{
    public static class AttendanceStatusExtensions
    {
        /// <summary>
        /// Convierte el estado en una letra corta para reportes (PDF, Excel, Tablas compactas).
        /// Ej: Presente -> "P"
        /// </summary>
        public static string ToLetter(this AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Present => "P",
                AttendanceStatus.Absent => "A",
                AttendanceStatus.Late => "T", // T de Tardanza
                AttendanceStatus.Excused => "J", // J de Justificado
                AttendanceStatus.Withdrawn => "R", // R de Retirado
                _ => "-"  // Valor por defecto/Error
            };
        }

        /// <summary>
        /// Devuelve el color Hexadecimal para interfaces gráficas (QuestPDF, CSS).
        /// </summary>
        public static string ToColorHex(this AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Present => "#28a745", // Verde (Bootstrap Success)
                AttendanceStatus.Absent => "#dc3545", // Rojo (Bootstrap Danger)
                AttendanceStatus.Late => "#fd7e14", // Naranja (Bootstrap Warning)
                AttendanceStatus.Excused => "#17a2b8", // Cian (Bootstrap Info)
                AttendanceStatus.Withdrawn => "#6c757d", // Gris (Bootstrap Secondary)
                _ => "#000000"  // Negro
            };
        }

        /// <summary>
        /// Lógica de Negocio: Determina si el estado cuenta positivamente para el porcentaje de asistencia.
        /// Regla: Presente y Tardanza SUMAN. Ausente y Justificado NO SUMAN.
        /// </summary>
        public static bool CountsAsPresent(this AttendanceStatus status)
        {
            return status == AttendanceStatus.Present ||
                   status == AttendanceStatus.Late;
        }

        /// <summary>
        /// Devuelve el nombre completo en Español para mostrar en etiquetas o tooltips.
        /// </summary>
        public static string ToFriendlyName(this AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Present => "Presente",
                AttendanceStatus.Absent => "Ausente",
                AttendanceStatus.Late => "Llegada Tarde",
                AttendanceStatus.Excused => "Justificado",
                AttendanceStatus.Withdrawn => "Retirado",
                _ => "Desconocido"
            };
        }

        /// <summary>
        /// Devuelve la clase CSS de Bootstrap para badges (etiquetas).
        /// Uso: class="badge @status.ToBadgeClass()"
        /// </summary>
        public static string ToBadgeClass(this AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Present => "bg-success",
                AttendanceStatus.Absent => "bg-danger",
                AttendanceStatus.Late => "bg-warning text-dark",
                AttendanceStatus.Excused => "bg-info text-dark",
                AttendanceStatus.Withdrawn => "bg-secondary",
                _ => "bg-dark"
            };
        }
    }
}
