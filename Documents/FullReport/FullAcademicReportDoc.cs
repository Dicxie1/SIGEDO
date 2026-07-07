using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Asistencia.Documents.FullReport.Models;
using Asistencia.Models.ViewModels;
namespace Asistencia.Documents.FullReport
{
    public class FullAcademicReportDoc : IDocument
    {
        private readonly FullAcademicReportViewModel _model;
        private readonly string UraccanBlue = "#003876";
        private readonly string UraccanOrange = "#F7931E";
        private readonly string BorderColorLight = Colors.Grey.Lighten2;

        public FullAcademicReportDoc( FullAcademicReportViewModel model)
        {
            _model = model;
        }
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.Letter);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.SegoeUI));
                page.Header().Element(ComposeGlobalHeader);
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item()
                        .Text("I. Datos General".ToUpper())
                        .Bold().FontSize(12).FontColor(UraccanBlue);
                    col.Item().Element(ComposeCourseInfoSection);
                    col.Item().PaddingBottom(15);
                    col.Item().PaddingBottom(15).Text("II. Avance Programático".ToUpper())
                        .Bold()
                        .FontSize(12)
                        .FontColor(UraccanBlue);
                    if(_model.GradeBook?.Terms != null)
                    {
                        foreach (var term in _model.GradeBook.Terms)
                        {
                            // Verificamos si tenemos datos de avance para este parcial en el diccionario
                            if (_model!.ProgrammaticProgress!.TryGetValue(term.TermId, out var progModel))
                            {
                                col.Item().PaddingTop(5).PaddingBottom(2)
                                    .Text($"Estadísticas - {term.Name}".ToUpper())
                                    .Bold().FontSize(9).FontColor(UraccanOrange);

                                // Pasamos el modelo específico hallado al componente
                                col.Item().PaddingBottom(10).Element(container => ComposeProgrammaticStatisticsSection(container, progModel));
                            }
                        }
                    }
                });

                page.Footer().Element(ComposeGlobalFooter);
            });
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.Legal.Landscape());
                page.Header().Element(ComposeGlobalHeader);
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().PaddingBottom(5).PaddingTop(5).Text("III. Asistencia".ToUpper()).Bold().FontSize(12).FontColor(UraccanBlue);
                    col.Item().Element(ComposeAttendanceTableSession);
                    col.Item().PageBreak();
                    col.Item().Padding(15)
                        .Text("IV. Calificación".ToUpper()).Bold().FontSize(12).FontColor(UraccanBlue);
                    col.Item().Element(ComposeGradeBookSection);
                });
                page.Footer().Element(ComposeGlobalFooter);
            });
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.Letter);
                page.Header().Element(ComposeGlobalHeader);
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().PaddingBottom(5).Text("V. Atención estudiantil".ToUpper()).Bold().FontSize(12).FontColor(UraccanBlue);
                    col.Item().Element(ComposeAttentionRecordsSection);
                });
                page.Footer().Element(ComposeGlobalFooter);
            });
            
        }
        #region 1. Compoente de encabezado Global
        public void ComposeGlobalHeader(IContainer container)
        {
            container.BorderBottom(1).BorderColor(BorderColorLight).PaddingBottom(5).Row(row =>
            {
                row.ConstantItem(65).Height(50).Image("wwwroot/img/logo.png");
                row.RelativeItem().PaddingLeft(10).Column(col =>
                {
                    col.Item().Text("UNIVERSIDAD DE LAS REGIONES AUTÓNOMAS DE LA COSTA CARIBE DE NICARAGUA").Bold().FontSize(16).FontColor(UraccanBlue).AlignCenter();
                    col.Item().Text("INFORME DE ASIGNATURA").Bold().FontSize(13).FontColor(UraccanBlue).AlignCenter();
                });
            });
        }
        #endregion
        #region 2. Reutilización de Datos de Asignatura (Tu código de Avance Programático)
        void ComposeCourseInfoSection(IContainer container)
        {
            var referenceProgress = _model.ProgrammaticProgress.Values.FirstOrDefault();
            container.Background(Colors.Grey.Lighten4).Padding(10).Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100); columns.RelativeColumn();
                    columns.ConstantColumn(100); columns.RelativeColumn();
                });
                table.Cell().Text("Docente:").Bold();
                table.Cell().Text("Dicxie Danuard Madrigal Brack");
                table.Cell().Text("Asignatura:").Bold();
                table.Cell().Text($"{referenceProgress.CourseName}"); //_model.ProgrammaticProgress.CourseName ?? "Asignatura sin asignar"
                table.Cell().Text("Carrera:").Bold();
                table.Cell().Text(referenceProgress?.CarrerName ?? "Ingeniería");

                table.Cell().Text("Semestre:").Bold();
                table.Cell().Text($"{referenceProgress?.semester} Semestre ");
            });
        }
        #endregion
        #region 3. Reutilización de Tabla Estadística (Tu código de Avance Programático)
        void ComposeProgrammaticStatisticsSection(IContainer container, ProgrammaticProgressViewModel? progModel)
        {
            
            container.Table(table =>
            {
                table.ColumnsDefinition(col =>
                {
                    for (int i = 0; i < 21; i++) col.RelativeColumn();
                });
                string[] headers = { "Matrícula Inicial", "Matrícula Final", "Aprobados", "% Aprobados", "Reprobados", "% Reprobados", "No Examinados" };
                foreach (var h in headers)
                {
                    table.Cell().ColumnSpan(3).Background(UraccanBlue).Border(1).BorderColor(Colors.White).Padding(4).AlignCenter().Text(h).FontColor(Colors.White).Bold().FontSize(8);
                }

                for (int i = 0; i < 7; i++)
                {
                    string[] sub = { "H", "M", "T" };
                    foreach (var s in sub)
                        table.Cell().Background(UraccanBlue).Border(1).BorderColor(Colors.White).Padding(2).AlignCenter().Text(s).FontColor(Colors.White).FontSize(8).Bold();
                }
                AddMetricCells(table, progModel.Initial.Male, progModel.Initial.Female, progModel.Initial.Total, false);
                AddMetricCells(table, progModel.Final.Male, progModel.Final.Female, progModel.Final.Total, false);
                AddMetricCells(table, progModel.Approved.Male, progModel.Approved.Female, progModel.Approved.Total, false);
                AddMetricCells(table, progModel.ApprovedPct.Male, progModel.ApprovedPct.Female, progModel.ApprovedPct.Total, true);
                AddMetricCells(table, progModel.Failed.Male, progModel.Failed.Female, progModel.Failed.Total, false);
                AddMetricCells(table, progModel.FailedPct.Male, progModel.FailedPct.Female, progModel.FailedPct.Total, true);
                AddMetricCells(table, progModel.NotExamined.Male, progModel.NotExamined.Female, progModel.NotExamined.Total, false);
            });
        }
        private void AddMetricCells(TableDescriptor table, double h, double m, double t, bool isPct)
        {
            string sfx = isPct ? "%" : "";
            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text($"{h}{sfx}").FontSize(6);
            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text($"{m}{sfx}").FontSize(6);
            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Background(Colors.Grey.Lighten4).AlignCenter().Text($"{t}{sfx}").Bold().FontSize(6);
        }
        #endregion
        #region  4. Reutilización de Matriz de Asistencia (Tu código de AttendanceDocument)
        void ComposeAttendanceTableSession(IContainer container)
        {
            var attModel = _model.Attendance;

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(4);
                    foreach (var d in attModel.Dates ) columns.ConstantColumn(20);
                    columns.ConstantColumn(40);
                });
                table.Header(header => 
                {
                    header.Cell().Background(UraccanBlue).Padding(3).AlignCenter().Text("#").FontColor(Colors.White).Bold();
                    header.Cell().Background(UraccanBlue).Padding(3).Text("Carnet").FontColor(Colors.White).Bold();
                    header.Cell().Background(UraccanBlue).Padding(3).Text("Estudiantes").FontColor(Colors.White).Bold();
                    foreach (var date in attModel.Dates)
                    {
                        header.Cell().Background(UraccanBlue).Padding(2).RotateLeft().AlignCenter().Text(date.ToString("dd/MM")).FontSize(7).FontColor(Colors.White).Bold();
                    }
                    header.Cell().Background(UraccanBlue).Padding(3).AlignCenter().Text("% Asist").FontColor(Colors.White).Bold().FontSize(8);
                });
                int index = 1;
                foreach (var student in attModel.Students)
                {
                    var rowBg = index % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;

                    table.Cell().Background(rowBg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(index.ToString());
                    table.Cell().Background(rowBg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{student.StudentId}");
                    table.Cell().Background(rowBg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{student.StudentName}");

                    foreach (var date in attModel.Dates)
                    {
                        string status = student.AttendanceLog.ContainsKey(date) ? student.AttendanceLog[date] : "-";
                        string statusColor = status switch { "P" => Colors.Green.Medium, "A" => Colors.Red.Medium, "T" => Colors.Orange.Medium, "J" => Colors.Blue.Medium, _ => Colors.Black };

                        table.Cell().Background(rowBg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignCenter().AlignMiddle().Text(status).FontColor(statusColor).Bold().FontSize(8);
                    }

                    string pctColor = student.AttendancePercentage < 80 ? Colors.Red.Medium : Colors.Black;
                    table.Cell().Background(rowBg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text($"{student.AttendancePercentage}%").FontColor(pctColor).Bold().FontSize(8);
                    index++;
                }

            });
        }
        #endregion
        #region 5 Sección Nueva: Bitácora de Atención Estudiantil
        void ComposeAttentionRecordsSection(IContainer container)
        {
            if (!_model.AttentionRecord!.Any())
            {
                container.Background(Colors.Grey.Lighten4).Padding(5).AlignCenter().Text("No se registran eventos de atención o tutorías especiales en este periodo.").Italic().FontColor(Colors.Grey.Medium);
                return;
            }
            container.Table(table => 
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(65);  // Fecha
                    columns.RelativeColumn(3);   // Participantes
                    columns.RelativeColumn(5);   // Observación / Incidencia
                    columns.ConstantColumn(75);  // Categoría/Prioridad
                    columns.ConstantColumn(65);  // Estado
                });
                table.Header(header =>
                {
                    string[] attHeaders = { "Fecha", "Estudiantes Atendidos", "Observación Pedagógica / Acuerdos", "Clasificación", "Estado" };
                    foreach (var h in attHeaders)
                        header.Cell().Background(UraccanBlue).Padding(4).Text(h).FontColor(Colors.White).Bold();
                });
                foreach (var record in _model.AttentionRecord)
                {
                    table.Cell().BorderBottom(1).BorderColor(BorderColorLight).Padding(5).Text(record.DateStr);

                    // Renderizar sublista de alumnos implicados
                    table.Cell().BorderBottom(1).BorderColor(BorderColorLight).Padding(5).Column(c =>
                    {
                        foreach (var name in record.StudentNames)
                            c.Item().Text($"• {name}").FontSize(8.5F);
                    });

                    table.Cell().BorderBottom(1).BorderColor(BorderColorLight).Padding(5).Text(record.Observation);

                    table.Cell().BorderBottom(1).BorderColor(BorderColorLight).Padding(5).Column(c =>
                    {
                        c.Item().Text($"Cat: {record.Category}").Bold().FontSize(8);
                        c.Item().Text($"Prioridad: {record.Priority}").FontColor(record.Priority == "Alta" ? Colors.Red.Medium : Colors.Black).FontSize(8);
                    });

                    string stateColor = record.Status == "Resuelto" ? Colors.Green.Medium : Colors.Red.Medium;
                    table.Cell().BorderBottom(1).BorderColor(BorderColorLight).Padding(5).Text(record.Status).FontColor(stateColor).Bold();
                }
            });
        }
        #endregion
        #region 6 
        void ComposeGradeBookSection(IContainer container)
        {
            var gradebook = _model.GradeBook;
            if (gradebook?.Terms == null || !gradebook.Terms.Any())
            {
                container.Background(Colors.Grey.Lighten4).Padding(15).AlignCenter().Column(col =>
                {
                    col.Item().Text("⚠️").Bold().FontSize(20).AlignCenter();
                    col.Item().Text("No hay registro de califiacaciones estructurado para la asignatura ").Italic().FontColor(Colors.Grey.Darken1).FontSize(14);
                });
                return;
            }
            int totalAssignmetsCount = gradebook.Terms.Sum(t => (t.Assignments?.Count ?? 0));
            int totalColumns = 1 + totalAssignmetsCount + 1;
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    foreach(var term in gradebook.Terms)
                    {
                        foreach(var task in term.Assignments)
                        {
                            columns.ConstantColumn(30);
                        }
                        columns.ConstantColumn(35);
                        columns.ConstantColumn(36);
                    }
                    columns.ConstantColumn(38);
                });
                table.Header(header =>
                {
                    
                    
                    header.Cell().Background(Colors.Green.Lighten3).Padding(4).AlignMiddle().Text("").Bold().FontSize(8).FontColor(Colors.Green.Darken3)
                    ;
                    foreach(var term in gradebook.Terms)
                    {
                        int termColSpan = (term.Assignments?.Count ?? 0) + 2;
                        header.Cell()
                            .ColumnSpan((uint)termColSpan)
                            .Background(UraccanBlue)
                            .Border(1)
                            .BorderColor(Colors.White)
                            .Padding(3)
                            .AlignCenter().Text(t =>
                            {
                                t.Span($"{term.Name} ({term.Weight}%)").FontColor(Colors.White).Bold().FontSize(8);
                            });
                    }
                    header.Cell()
                        .Background(UraccanBlue)
                        .Border(1).BorderColor(Colors.White)
                        .Padding(4).AlignCenter().AlignMiddle()
                        .Text("Nota\nFinal").Bold().FontColor(Colors.White).FontSize(8).AlignCenter();
                    header.Cell().Background(Colors.Green.Lighten3).Padding(2).Text("Nombre de Estudiantes").Bold().FontSize(8).FontColor(Colors.Green.Darken3);
                    var isfirst = 1;
                    foreach (var term in gradebook.Terms)
                    {   
                        foreach (var task in term.Assignments!)
                        {
                           
                            var taskBg = task.IsExam ? Colors.Red.Lighten5 : Colors.White;
                            var taskTextColor = task.IsExam ? Colors.Red.Darken3 : Colors.Blue.Darken3;
                            string shortTitle = task.Title.Length > 9 ? task.Title.Substring(0, 7) + ".." : task.Title;
                            
                            header.Cell().Background(taskBg).Border(1).BorderColor(BorderColorLight).Padding(2).AlignCenter().AlignMiddle().Column(c =>
                            {
                                c.Item().Text(shortTitle).Bold().FontColor(taskTextColor).FontSize(6.5f).LineHeight(0.9f);
                                c.Item().Text(text =>
                                {
                                    text.Span("pts").FontSize(5.4F).FontColor(Colors.Grey.Darken1).Superscript();
                                    text.Span($"/{task.MaxPoints}").FontColor(Colors.Grey.Darken1).FontSize(6);

                                });
                            });
                        }
                        header.Cell().Background(Colors.Grey.Lighten3).Border(1).BorderColor(BorderColorLight).Padding(2).AlignCenter().AlignMiddle()
                     .Text("Total\nAcum").Bold().FontSize(6.5f).FontColor(Colors.Green.Darken3).AlignCenter();
                        header.Cell().Background(Colors.Grey.Lighten3).Border(1).BorderColor(BorderColorLight).Padding(2).AlignCenter().AlignMiddle()
                            .Text($"Total\n{term.Name}").Bold().FontSize(7).AlignCenter();
                        isfirst++;
                    }
                    
                    header.Cell()
                      .Background(UraccanBlue)
                      .Border(1).BorderColor(Colors.White)
                      .Padding(4).AlignCenter().AlignMiddle()
                       .Text("Nota\nFinal").Bold().FontColor(Colors.White).FontSize(8).AlignCenter();
                });
                int studentIndex = 1;

                foreach(var student in gradebook.Students)
                {
                    var rowBg = studentIndex % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                    table.Cell().Background(rowBg).Border(1).BorderColor(BorderColorLight).Padding(4).AlignMiddle().Column(c =>
                    {
                        c.Item().Text(student.StudentFullName ?? "SIn Nombre").Bold().FontSize(6.7F);
                        c.Item().Text(student.StudentId ?? "N/A").Bold().FontSize(7).FontColor(Colors.Grey.Lighten1);
                       
                    });
                    foreach(var term in gradebook.Terms)
                    {
                        double taskSum = 0;
                        double termSum = 0;
                        foreach(var task in term.Assignments!)
                        {
                            double? gradeVal = student.Grades != null && student!.Grades!.ContainsKey(task.AssignmentId) ? student.Grades[task.AssignmentId] : null;
                            if (gradeVal.HasValue)
                            {
                                termSum += gradeVal.Value;
                                if (!task.IsExam) taskSum += gradeVal.Value;
                            }
                            var cellBg = task.IsExam ? Colors.Red.Lighten5 : rowBg;
                            string gradeText = gradeVal.HasValue ? gradeVal.Value.ToString("0.#") : "-";
                            table.Cell().Background(cellBg).Border(1).BorderColor(BorderColorLight).Padding(2).AlignCenter().AlignMiddle().Text(gradeText).FontSize(7.5f).FontColor(task.IsExam ? Colors.Red.Darken3 : Colors.Black).AlignCenter();
                        }
                        table.Cell().Background(Colors.Grey.Lighten3).Border(1).BorderColor(BorderColorLight).Padding(2).AlignCenter().AlignMiddle()
                     .Text(taskSum.ToString("0.#")).Bold().FontSize(7.5F).FontColor(Colors.Green.Darken3).AlignCenter();
                        table.Cell().Background(Colors.Grey.Lighten3).Border(1).BorderColor(BorderColorLight).Padding(2).AlignCenter().AlignMiddle().Text($" {termSum.ToString("#.##")}").Bold().FontSize(7.5F).AlignCenter();
                    }
                    string finalGradeText = student.FinalGrade.ToString("0.#");
                    table.Cell().Background(UraccanBlue).Border(1).BorderColor(Colors.White).Padding(4).AlignCenter().AlignMiddle().Text(finalGradeText).Bold().FontColor(Colors.White).FontSize(8.5f);

                    studentIndex++;
                }
            });
        }
        #endregion
        #region 7
        void ComposeGlobalFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text($"Reporte generado automáticamente el {DateTime.Now:dd/MM/yyyy hh:mm tt}").FontSize(7);
                row.RelativeItem().AlignRight().Text(x =>
                {
                    x.Span("Página ").FontSize(8);
                    x.CurrentPageNumber().FontSize(8);
                    x.Span(" de ").FontSize(8);
                    x.TotalPages().FontSize(8);
                });
            });
        }
        #endregion
        
    }
}
