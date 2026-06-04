using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Asistencia.Models.ViewModels;
namespace Asistencia.Documents.ProgrammaticProgress
{
    public class ProgrammatiProgressDoc : IDocument
    {
        private readonly ProgrammaticProgressViewModel _model;
        public ProgrammatiProgressDoc(ProgrammaticProgressViewModel model)
        {
            _model = model;
        }
        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page => {
                    page.Margin(20);
                    page.Size(PageSizes.Legal.Landscape());
                    page.Header().Element(HeaderContainer);
                    page.Content().
                    PaddingVertical(10)
                    .Column(col =>
                    {
                        col.Item().Element(ComposeCourseInfo);
                        col.Item().Element(ComposeStatisticsTable);
                    });
                });

        }
        void HeaderContainer(IContainer container)
        {
            container.Row(row =>
            {
                row.ConstantItem(70)
                   .Height(70)
                   .Image("wwwroot/img/logo.png"); // Asegúrate de que esta ruta local sea válida en producción

                row.RelativeItem()
                   .Column(col =>
                   {
                       col.Item().Text("UNIVERSIDAD DE LAS REGIONES AUTÓNOMAS DE LA COSTA CARIBE DE NICARAGUA")
                          .FontSize(13)
                          .Bold()
                          .AlignCenter()
                          .FontColor(UraccanColors.Primary);

                       col.Item().Text("URACCAN")
                          .FontSize(13)
                          .Bold()
                          .AlignCenter()
                          .FontColor(UraccanColors.Primary);

                       col.Item().PaddingTop(5);

                       col.Item().Text("AVANCE PROGRAMÁTICO")
                          .Bold()
                          .AlignCenter()
                          .FontSize(11)
                          .FontColor(UraccanColors.Primary);
                   });
            }
            
            );
        }
        void ComposeCourseInfo(IContainer container)
        {
            container
                .Background(UraccanColors.Light)
                .Padding(10)
                .Border(1)
                .BorderColor(UraccanColors.Border)
                .Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(105); columns.RelativeColumn();
                            columns.ConstantColumn(105); columns.RelativeColumn();
                            columns.ConstantColumn(105); columns.RelativeColumn();
                        });

                        // Fila 1 - Datos dinámicos desde el modelo y datos de contexto
                        table.Cell().Text("Docente:").Bold();
                        table.Cell().Text("Dicxie Danuard Madrigal Brack");

                        table.Cell().Text("Carrera:").Bold();
                        table.Cell().Text($"{_model.CarrerName}");

                        table.Cell().Text("Asignatura:").Bold();
                        table.Cell().Text(_model.CourseName ?? "Introducción a la Computación");

                        // Fila 2
                        table.Cell().Text("Año Académico:").Bold();
                        table.Cell().Text($"{_model.AcademicYear}");

                        table.Cell().Text("Semestre:").Bold();
                        table.Cell().Text($"{_model.semester} Semetre");

                        table.Cell().Text("Modalidad:").Bold();
                        table.Cell().Text("Presencial");

                        // Fila 3
                        table.Cell().Text("Horas Teóricas:").Bold();
                        table.Cell().Text("12");

                        table.Cell().Text("Horas Prácticas:").Bold();
                        table.Cell().Text("18");

                        table.Cell().Text("Corte Evaluativo:").Bold();
                        table.Cell().Text(_model.TermName ?? $"Corte {_model.TermId}");
                    });

                    col.Item().PaddingTop(8)
                        .Text($"Fecha de generación: {DateTime.Now:dd/MM/yyyy hh:mm tt}")
                        .FontColor(Colors.Grey.Medium)
                        .AlignCenter()
                        .FontSize(9);
                });
        }
        private void ComposeStatisticsTable(IContainer container){
            container.Table(table =>
            {
                // 24 Columnas proporcionales unificadas (3 subcolumnas H-M-T x 8 categorías principales)
                table.ColumnsDefinition(col =>
                {
                    for (int i = 0; i < 21; i++)
                        col.RelativeColumn();
                });

                // --- NIVEL ENCABEZADO 1 ---
                string[] headers = {
                    "Matrícula Inicial", "Matrícula Final",
                    "Aprobados", "% Aprobados", "Reprobados", "% Reprobados", "No Examinados"
                };

                // Renderizamos las primeras cabeceras (cada una abarca 3 columnas)
                foreach (var headerText in headers)
                {
                    table.Cell().ColumnSpan(3).Element(HeaderStyle).Text(headerText).FontColor(Colors.White).Bold().FontSize(9);
                }

                // Espacio extra para completar las 24 columnas si fuese necesario, o ajustado a tus 7 campos de la vista web
                // Nota: En tu vista web renderizas 7 bloques principales en lugar de 8. He removido "% Retención" para que calce con tu HTML.
                // Ajustamos la última columna (No Examinados o el que decidas) para ocupar el resto (6 columnas) o reducimos la malla a 21 columnas si prefieres.
                // Para mantener simetría perfecta con tus 7 bloques de la vista Razor, usemo 21 columnas reales en total.

                // --- RE-PROCESADO DINÁMICO DE SUBENCABEZADOS (H - M - T) ---
                // Son 7 categorías en tu HTML: Inicial, Final, No Exam, Aprobados, % Aprob, Reprobados, % Reprob.
                for (int i = 0; i < 7; i++)
                {
                    AddSubHeaderCell(table, "H");
                    AddSubHeaderCell(table, "M");
                    AddSubHeaderCell(table, "T");
                }

                // --- RENDEREADO DE VALORES REALES DESDE EL MODELO ---
                AddMetricGroupValues(table, _model.Initial.Male, _model.Initial.Female, _model.Initial.Total, false);
                AddMetricGroupValues(table, _model.Final.Male, _model.Final.Female, _model.Final.Total, false);
                
                AddMetricGroupValues(table, _model.Approved.Male, _model.Approved.Female, _model.Approved.Total, false);
                AddMetricGroupValues(table, _model.ApprovedPct.Male, _model.ApprovedPct.Female, _model.ApprovedPct.Total, true);
                AddMetricGroupValues(table, _model.Failed.Male, _model.Failed.Female, _model.Failed.Total, false);
                AddMetricGroupValues(table, _model.FailedPct.Male, _model.FailedPct.Female, _model.FailedPct.Total, true);
                AddMetricGroupValues(table, _model.NotExamined.Male, _model.NotExamined.Female, _model.NotExamined.Total, false);
            });

        }
        private IContainer HeaderStyle(IContainer container)
        {
            return container
                .Background(UraccanColors.Primary)
                .Border(1)
                .BorderColor(Colors.White)
                .Padding(5)
                .AlignCenter()
                .AlignMiddle();
        }
        private void AddMetricGroupValues(TableDescriptor table, double male, double female, double total, bool isPercentage)
        {
            string suffix = isPercentage ? "%" : "";

            table.Cell().Element(CellStyle).Text($"{male}{suffix}").FontSize(9);
            table.Cell().Element(CellStyle).Text($"{female}{suffix}").FontSize(9);
            table.Cell().Element(CellStyle).Background(Colors.Grey.Lighten4).Text($"{total}{suffix}").FontSize(9).Bold();
        }
        private void AddSubHeaderCell(TableDescriptor table, string label)
        {
            table.Cell()
                .Background(UraccanColors.Primary)
                .Border(1)
                .BorderColor(Colors.White)
                .Padding(3)
                .AlignCenter()
                .AlignMiddle()
                .Text(label).FontColor(Colors.White).FontSize(8).Bold();
        }
        public void AddHeader(TableDescriptor table, string background)
        {
            for(int i =0; i < 8; i++)
                table.Cell().AlignCenter().AlignMiddle().Background(background).Table(table =>
                {
                    addColumn(table, background);
                });
        }
       public void addColumn(TableDescriptor table, string background)
        {
            table.ColumnsDefinition(col =>
            {
                col.RelativeColumn();
                col.RelativeColumn();
                col.RelativeColumn();
            });
            table.Cell().AlignCenter().AlignMiddle().Text("M").BackgroundColor(background).FontColor(Colors.White);
            table.Cell().AlignCenter().AlignMiddle().Text("H").BackgroundColor(background).FontColor(Colors.White);
            table.Cell().AlignCenter().AlignMiddle().Text("T").BackgroundColor(background).FontColor(Colors.White);
        }
        public void AddRow(TableDescriptor table, string label, int male, int female, int total, string backgroundColor)
        {
            table.Cell()
                .Background(backgroundColor)
                .Element(CellStyle)
                .Table(table =>
                {

                    table.ColumnsDefinition(col =>
                    {
                        col.RelativeColumn();
                        col.RelativeColumn();
                        col.RelativeColumn();
                    });
                    table.Cell().AlignCenter().AlignMiddle().Text("2");
                    table.Cell().AlignCenter().AlignMiddle().Text("3");
                    table.Cell().AlignCenter().AlignMiddle().Text("5");
                });
            table.Cell()
                .Background(backgroundColor)
                .Element(CellStyle)
                .AlignCenter()
                .Table(table =>
                {
                    
                    table.ColumnsDefinition(col =>
                    {
                        col.RelativeColumn();
                        col.RelativeColumn();
                        col.RelativeColumn();
                    });
                    table.Cell().AlignCenter().AlignMiddle().Text("2");
                    table.Cell().AlignCenter().AlignMiddle().Text("3");
                    table.Cell().AlignCenter().AlignMiddle().Text("5");
                });
            table.Cell()
        .Background(backgroundColor)
        .Element(CellStyle)
        .AlignCenter()
        .Table(table =>
        {

            table.ColumnsDefinition(col =>
            {
                col.RelativeColumn();
                col.RelativeColumn();
                col.RelativeColumn();
            });
            table.Cell().AlignCenter().AlignMiddle().Text("2");
            table.Cell().AlignCenter().AlignMiddle().Text("3");
            table.Cell().AlignCenter().AlignMiddle().Text("5");
        });

            table.Cell()
                .Background(backgroundColor)
                .Element(CellStyle)
                .AlignCenter()
                .Table(table =>
                {

                    table.ColumnsDefinition(col =>
                    {
                        col.RelativeColumn();
                        col.RelativeColumn();
                        col.RelativeColumn();
                    });
                    table.Cell().AlignCenter().AlignMiddle().Text("2");
                    table.Cell().AlignCenter().AlignMiddle().Text("3");
                    table.Cell().AlignCenter().AlignMiddle().Text("5");
                });
            table.Cell()
                .Background(backgroundColor)
                .Element(CellStyle)
                .AlignCenter()
                .Table(table =>
                {

                    table.ColumnsDefinition(col =>
                    {
                        col.RelativeColumn();
                        col.RelativeColumn();
                        col.RelativeColumn();
                    });
                    table.Cell().AlignCenter().AlignMiddle().Text("2");
                    table.Cell().AlignCenter().AlignMiddle().Text("3");
                    table.Cell().AlignCenter().AlignMiddle().Text("5");
                });
            table.Cell()
                .Background(backgroundColor)
                .Element(CellStyle)
                .AlignCenter()
                .Table(table =>
                {

                    table.ColumnsDefinition(col =>
                    {
                        col.RelativeColumn();
                        col.RelativeColumn();
                        col.RelativeColumn();
                    });
                    table.Cell().AlignCenter().AlignMiddle().Text("2");
                    table.Cell().AlignCenter().AlignMiddle().Text("3");
                    table.Cell().AlignCenter().AlignMiddle().Text("5");
                });
            table.Cell()
                .Background(backgroundColor)
                .Element(CellStyle)
                .AlignCenter()
                .Table(table =>
                {

                    table.ColumnsDefinition(col =>
                    {
                        col.RelativeColumn();
                        col.RelativeColumn();
                        col.RelativeColumn();
                    });
                    table.Cell().AlignCenter().AlignMiddle().Text("2");
                    table.Cell().AlignCenter().AlignMiddle().Text("3");
                    table.Cell().AlignCenter().AlignMiddle().Text("5");
                });
            table.Cell()
                .Background(backgroundColor)
                .Element(CellStyle)
                .AlignCenter()
                .Table(table =>
                {

                    table.ColumnsDefinition(col =>
                    {
                        col.RelativeColumn();
                        col.RelativeColumn();
                        col.RelativeColumn();
                    });
                    table.Cell().AlignCenter().AlignMiddle().Text("2");
                    table.Cell().AlignCenter().AlignMiddle().Text("3");
                    table.Cell().AlignCenter().AlignMiddle().Text("5");
                });
        }
        private IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5);
        }
        
}

}
