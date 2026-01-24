using Asistencia.Services.Interfaces;
using ClosedXML.Excel;
using Asistencia.Models;
using System;
using ClosedXML.Attributes;
namespace Asistencia.Documents;
public class ExcelGradeBookDocument : IGradebookExportService
{
    
    public byte[] GenerateExportReport(Course course, List<AcademicTerm> terms, List<Enrollment> enrollments)
    {
        using (var workbook = new XLWorkbook())
        {
            var ws = workbook.Worksheets.Add("Sábana de Notas");
            
            // 1. ENCABEZADOS GENERALES
            ws.Cell(1, 1).Value = "REPORTE DE CALIFICACIONES";
            ws.Cell(2, 1).Value = course?.Subject?.SubjetName;
            ws.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            var titleRange = ws.Range("A1:K1");
            titleRange.Merge().Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 16;
            titleRange.Style.Font.FontColor = XLColor.FromHtml("#0d6efd");
            titleRange.Style.Font.FontFamilyNumbering = XLFontFamilyNumberingValues.Modern;

            var subjectName = ws.Range("A2:E2");
            subjectName.Merge().Style.Font.Bold = true;
            subjectName.Style.Font.FontColor = XLColor.FromHtml("#FF0081");

            var fecha = ws.Range("A3:E3");
            fecha.Merge().Style.Font.Italic = true;

            // 2. ENCABEZADOS DE TABLA
            int headerRow = 7;
            SetupTableHeaders(ws, headerRow, terms, out int finalCol);
            
            // 3. DATOS DE ESTUDIANTES
            FillStudentData(ws, headerRow + 3, enrollments, terms);

            // 4. ESTILOS FINALES
            var lastRow = headerRow + 3 + enrollments.Count - 1;
            var tableRange = ws.Range(headerRow, 1, lastRow, finalCol);
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            
            
            ws.Column(1).Width = 3;
            ws.Column(2).Width = 20;
            ws.Column(3).Width = 50; // Nombre estudiante
            ws.Column(finalCol).Width = 10;
            ws.Row(1).Height = 30;
            ws.Row(2).Height = 22;
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
    // Método privado para limpiar el código principal
    private void SetupTableHeaders(IXLWorksheet ws, int headerRow, List<AcademicTerm> terms, out int currentCol)
    {
        ws.Cell(headerRow, 1).Value = "#";
        ws.Cell(headerRow, 2).Value = "Carnet";
        ws.Cell(headerRow, 3).Value = "Nombre Completo";
        
        ws.Range(headerRow, 1, headerRow + 2, 1).Merge().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Range(headerRow, 2, headerRow + 2, 2).Merge().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Range(headerRow, 3, headerRow + 2, 3).Merge().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    
        currentCol = 4;
        foreach (var term in terms)
        {
            int startCol = currentCol;
            var t = ws.Cell(headerRow, currentCol).Value = $"{term.Name} ({term.WeightOnFinalGrade}%)";
            foreach (var task in term.Assignments)
            {
                var cell = ws.Range(headerRow + 1, currentCol, headerRow + 2, currentCol );
                cell.Merge();
                cell.Value = task.Title;
                cell.Style.Alignment.TextRotation = 90;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                
                if (task.IsExam) {
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#d2cbcbff");
                    cell.Style.Font.FontColor = XLColor.Red;
                }
                ws.Column(currentCol).Width = 3;
                currentCol++;
            }

            var subTotal = ws.Range(headerRow + 1, currentCol,headerRow + 2, currentCol);
            subTotal.Merge();
            subTotal.Value = "TOTAL";
            subTotal.Style.Font.Bold = true;
            subTotal.Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");
            subTotal.Style.Alignment.TextRotation = 90;
            ws.Column(currentCol).Width = 3;
            currentCol++;

            var headerRange = ws.Range(headerRow, startCol, headerRow, currentCol - 1);
            headerRange.Merge();
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#e9ecef");
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        }

        var finalHeader = ws.Cell(headerRow, currentCol);
        finalHeader.Value = "NOTA FINAL";

        ws.Range(headerRow, currentCol, headerRow + 2, currentCol).Merge();
        finalHeader.Style.Fill.BackgroundColor = XLColor.FromHtml("#0d6efd");
        finalHeader.Style.Font.FontColor = XLColor.White;
        finalHeader.Style.Font.Bold = true;
        finalHeader.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        finalHeader.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        
        ws.Row(headerRow + 2).Height = 120;
        
    }
    private void FillStudentData(IXLWorksheet ws, int startRow, List<Enrollment> enrollments, List<AcademicTerm> terms)
    {
        int row = startRow;
        int count = 1;

        foreach (var enrollment in enrollments)
        {
            ws.Cell(row, 1).Value = count++;
            ws.Cell(row, 2).Value = enrollment?.Student?.Id;
            ws.Cell(row, 3).Value = $"{enrollment?.Student?.LastName}, {enrollment?.Student?.Name}";

            var gradesDict = enrollment?.Grades.ToDictionary(g => g.AssignmentId, g => g.Score);
            int col = 4;
            double finalGradeAccumulator = 0;

            foreach (var term in terms)
            {
                double termSum = 0;
                foreach (var task in term.Assignments)
                {
                    if (gradesDict.ContainsKey(task.AssignmentId))
                    {
                        double score = gradesDict[task.AssignmentId];
                        ws.Cell(row, col).Value = score;
                        ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        termSum += score;
                    }
                    col++;
                }
                
                // Subtotal Corte
                var cellTotal = ws.Cell(row , col);
                cellTotal.Value = termSum;
                cellTotal.Style.Font.Bold = true;
                cellTotal.Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");
                
                finalGradeAccumulator += termSum; // Ajustar si usas ponderación compleja
                col++;
            }

            // Nota Final
            var finalCell = ws.Cell(row, col);
            finalCell.Value = finalGradeAccumulator;
            finalCell.Style.Font.Bold = true;
            finalCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            if (finalGradeAccumulator < 60) {
                finalCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#dc3545");
                finalCell.Style.Font.FontColor = XLColor.White;
            } else {
                finalCell.Style.Font.FontColor = XLColor.FromHtml("#0d6efd");
            }
            row++;
        }
    }



}
