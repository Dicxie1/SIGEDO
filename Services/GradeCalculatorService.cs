namespace Asistencia.Services;
using Asistencia.Models;
public class GradeCalculatorService
{
    // Calcula la nota final (0-100) de un estudiante en un CURSO completo
    public double CalculateFinalCourseGrade(int studentId, List<AcademicTerm> terms)
    {
        double finalGrade = 0;
        foreach (var term in terms)
        {
            // Calculamos la nota del corte (0-100)
            double termScore = CalculateTermScore(studentId, term);
            
            // Aplicamos el peso del corte (Ej: Si sacó 80 y el corte vale 30% -> suma 24 pts)
            finalGrade += termScore * (term.WeightOnFinalGrade / 100.0);
        }
        return Math.Round(finalGrade, 2);
    }

    // Calcula la nota de un solo CORTE (0-100)
    private double CalculateTermScore(int studentId, AcademicTerm term)
    {
        // 1. Separar actividades
        var accumTasks = term.Assignments.Where(a => !a.IsExam).ToList();
        var examTasks = term.Assignments.Where(a => a.IsExam).ToList();

        // 2. Calcular Acumulado
        double accumScore = GetWeightedScore(studentId, accumTasks); // 0 a 100
        double weightedAccum = accumScore * (term.AccumulatedWeight / 100.0);

        // 3. Calcular Examen
        double examScore = GetWeightedScore(studentId, examTasks); // 0 a 100
        double weightedExam = examScore * (term.ExamWeight / 100.0);

        return weightedAccum + weightedExam;
    }

    // Helper: Suma puntos obtenidos / puntos posibles y lo convierte a base 100
    private double GetWeightedScore(int enrollmenId, List<Assignment> assignments)
    {
        if (!assignments.Any()) return 0; // Si no hay tareas, tiene 0

        double pointsEarned = 0;
        double pointsPossible = 0;

        foreach (var task in assignments)
        {
            pointsPossible += task.MaxPoints;
            
            // Buscar la nota del estudiante en esta tarea
            var grade = task.Grades.FirstOrDefault(g => g.EnrollmentId == enrollmenId);
            if (grade != null)
            {
                pointsEarned += grade.Score;
            }
        }
        if (pointsPossible == 0) return 0;
        // Regla de tres simple para llevarlo a escala 100
        return (pointsEarned / pointsPossible) * 100;
    }
}