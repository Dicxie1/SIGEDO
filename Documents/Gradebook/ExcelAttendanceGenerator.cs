using ClosedXML.Excel;
using Asistencia.Models.DTOs;
namespace Asistencia.Documents.Gradebook
{
    public class ExcelAttendanceGenerator
    {
        public byte[] GenerateSheet(AttendanceSheetData data)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sabana de Asistencia");
            int totalColumns = 15;
            int colTotalAbsent = 0;
            var titleRange = worksheet.Range(1, 1, 1, totalColumns);
            if (!string.IsNullOrEmpty(data.LogoPath) && System.IO.File.Exists(data.LogoPath))
            {
                var logo = worksheet.AddPicture(data.LogoPath)
                            .MoveTo(worksheet.Cell(1, 1), 5, 5); // Insertar en A1 con 5px de margen

                // Ajustamos el tamaño de la imagen (puedes jugar con estos valores)
                logo.Width = 40;
                logo.Height = 40;

                // Para que el texto no quede pegado al logo, hacemos que las cabeceras comiencen 
                // a fusionarse desde la columna 3 (C) en lugar de la 1 (A).
                // Así las columnas A y B (Carnet y Nombre) quedan libres arriba para el logo.
                titleRange = worksheet.Range(1, 1, 1, totalColumns);
                titleRange.Merge().Value = "Universidad de las Regiones Autónomas de la Costa Caribe Nicaragüense (URACCAN)";
                // ... (Repites el cambio de inicio en columna 3 para subtitleRange, courseRange, etc.)
            }
            else
            {
                titleRange.Merge().Value = "Universidad de las Regiones Autónomas de la Costa Caribe Nicaragüense (URACCAN)";
            }
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 18;
            titleRange.Style.Font.FontColor = XLColor.DarkBlue; // Un toque de color institucional
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var subtitleRange = worksheet.Range(2, 1, 2, totalColumns);
            subtitleRange.Merge().Value = "Sábana de Asistencia";
            subtitleRange.Style.Font.Bold = true;
            subtitleRange.Style.Font.FontSize = 14;
            subtitleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Fila 3: Nombre de la Asignatura
            var courseRange = worksheet.Range(3, 1, 3, totalColumns);
            courseRange.Merge().Value = $"Asignatura: {data.CourseName}";
            courseRange.Style.Font.Bold = true;
            courseRange.Style.Font.FontSize = 11;
            courseRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            var hoursRange = worksheet.Range(4, 1, 4, totalColumns);
            hoursRange.Merge().Value = $"Total de Horas del Plan de Estudios: {data.CourseHour} hrs.";
            hoursRange.Style.Font.Bold = true;
            hoursRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            var dateRange = worksheet.Range(5, 1, 5, totalColumns);
            dateRange.Merge().Value = $"Reporte generado el: {DateTime.Now:dd/MM/yyyy hh:mm tt}";
            dateRange.Style.Font.Italic = true;
            dateRange.Style.Font.FontColor = XLColor.DimGray;
            dateRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            worksheet.Cell(6, 1).Value = "Carnet";
            worksheet.Cell(6, 2).Value = "Estudiante";

            int colIndex = 3;
            foreach(var date in data.SessionDates)
            {
                worksheet.Cell(6, colIndex).Value = date.ToString("dd/MM");
                worksheet.Cell(6, colIndex).Style.Alignment.TextRotation = 90;
                colIndex++;
            }
            worksheet.Cell(6, colIndex).Value = "Hrs. Ausente";
            colTotalAbsent = colIndex;
            worksheet.Cell(6, colIndex +1 ).Value = "% Ausente";
            int rowIndex = 7;
            foreach (var student in data.Students)
            {
                worksheet.Cell(rowIndex, 1).Value = student.Carnet;
                worksheet.Cell(rowIndex, 2).Value = student.FullName;
                int currentCol = 3;
                foreach (var date in data.SessionDates)
                {
                    var cell = worksheet.Cell(rowIndex, currentCol);
                    if (student.Attendances.TryGetValue(date, out string status))
                    {
                        cell.Value = status;
                        if (status == "A")
                        {
                            cell.Style.Font.FontColor = XLColor.Red;
                            cell.Style.Font.Bold = true;
                        }
                        if (status == "T")
                        {
                            cell.Style.Font.FontColor = XLColor.Orange;
                            cell.Style.Font.Bold = true;
                        }
                        if (status == "P")
                        {
                            cell.Style.Font.FontColor = XLColor.Green;
                            cell.Style.Font.Bold = true;
                        }
                    }
                    else
                    {
                        cell.Value = "-";
                    }
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentCol++;
                }
                var hoursCell = worksheet.Cell(rowIndex, currentCol);
                hoursCell.Value = student.TotalAbsences;
                hoursCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                var percentCell = worksheet.Cell(rowIndex, currentCol + 1);
                // Excel maneja el 100% como 1.0, así que dividimos entre 100.
                percentCell.Value = (int) student.AbsencePercentage / 100.0;
                percentCell.Style.NumberFormat.Format = "0%"; // Le decimos a Excel que es un porcentaje
                percentCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                if (student.TotalAbsences >= 20)
                {
                    percentCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                    percentCell.Style.Font.FontColor = XLColor.DarkRed;
                    percentCell.Style.Font.Bold = true;
                }

                rowIndex++;

            }
            if (data.Students.Count > 0)
            {
                var range = worksheet.Range(6, 1, rowIndex - 1, colIndex +1);
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            var headerRow = worksheet.Row(6);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.GreenRyb;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            worksheet.Column(1).AdjustToContents();
            worksheet.Column(2).AdjustToContents();
            
            if (colIndex > 3) worksheet.Columns(3, colIndex).Width = 4;
            worksheet.Column(colTotalAbsent).AdjustToContents();
            worksheet.Column(colTotalAbsent + 1).AdjustToContents();
            // 5. Devolver el archivo binario
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
