using Asistencia.Documents.Attendance.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SixLabors.Fonts;
using System.Drawing.Text;
namespace Asistencia.Documents.Attendance;

public class AttendanceDocument : IDocument
{
    private readonly AttendanceReportModel _model = new AttendanceReportModel();
    private readonly string UraccanBlue = "#003876";
    private readonly string UraccanOrange = "#F7931E";

    public AttendanceDocument( AttendanceReportModel model)
    {
        _model = model;
    }
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Size(PageSizes.Legal.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.SegoeUI));
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeBody);
                page.Footer().Element(ComposeFooter);
            });
    }
    void ComposeHeader(IContainer container)
    {

        container.Column(col =>
        {
            col.Item().Row(row => {
                row.ConstantItem(60).Image("wwwroot/img/logo.png");
                row.RelativeItem().AlignCenter().Text(a => { 
                    a.Span("Universidad de las Regiones Autonoma de la Costa Caribe Nicaragüense")
                        .Bold()
                        .FontColor(UraccanBlue)
                        .FontSize(28);
                    a.EmptyLine();
                    a.Span("REPORTE DE ASISTENCIA").Bold().FontSize(14).FontColor(UraccanOrange);
                });
                
            });
            col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            col.Item().Row(sub =>
            {
                sub.RelativeItem().Row(row =>
                {
                    // Logo (Placeholder) - Reemplaza con tu ruta real
                   
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Sistema de Gestión Académica").FontSize(10).FontColor(Colors.Grey.Medium);
                        col.Item().Text($"Recinto: {_model.Campus}").FontSize(10);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"Asignatura: {_model.CourseName}").Bold();
                        col.Item().Text($"Docente: {_model.ProfessorName}");
                        col.Item().Text($"Período: {_model.Term}");
                    });
                });
                
            });

        });
    }
    void ComposeBody(IContainer container)
    {
        container.PaddingVertical(10).Table(table =>
        {
            // 1. Definición de Columnas
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25); // #
                columns.RelativeColumn(3);  // Nombre Estudiante

                // Columnas dinámicas para cada fecha
                foreach (var date in _model.Dates)
                {
                    columns.ConstantColumn(18); // Ancho fijo para fechas
                }

                columns.ConstantColumn(40); // % Final
            });

            // 2. Encabezados de Tabla
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("#").FontColor(Colors.White).Bold().AlignCenter();
                header.Cell().Element(CellStyle).Text("Estudiante / Carnet").FontColor(Colors.White).Bold();
                

                foreach (var date in _model.Dates)
            {
                // Rotamos la fecha para ahorrar espacio si son muchas
                header.Cell().Element(CellStyle).RotateLeft().AlignCenter().Text(date.ToString("dd/MM")).FontSize(6).FontColor(Colors.White).Bold();
            }

            header.Cell().Element(CellStyle).Text("%").FontColor(Colors.White).Bold().AlignCenter();

            // Estilo base para el Header
            static IContainer CellStyle(IContainer container)
            {
                return container.Background("#003876").Border(1).BorderColor(Colors.White).Padding(2).AlignMiddle();
            }
            });

            IContainer HeaderStyle(IContainer c) => c
                .Background(UraccanBlue)
                .Border(1)
                .BorderColor(Colors.White)
                .Padding(2)
                .AlignMiddle()
                .AlignCenter();

            // 3. Filas de Datos
            foreach (var student in _model.Students)
            {
                var index = _model.Students.IndexOf(student) + 1;

                // Alternar color de fondo (Zebra Striping)
                var backgroundColor = index % 2 == 0 ? Colors.Green.Lighten4 : Colors.White;

                table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignCenter().Text(index.ToString());

                table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Column(col =>
                {
                    col.Item().Text(student.StudentName).Bold();
                    col.Item().Text(student.StudentId).FontSize(8).FontColor(Colors.Black);
                });

                // Celdas de Asistencia Dinámicas
                foreach (var date in _model.Dates)
                {

                    string status = student.AttendanceLog.ContainsKey(date) ? student.AttendanceLog[date] : "-";

                    // Color según estado
                    string color = status switch
                    {
                        "P" => Colors.Green.Medium, // Presente
                        "A" => Colors.Red.Medium,   // Ausente
                        "T" => Colors.Orange.Medium,// Tardanza
                        "J" => Colors.Blue.Medium,  // Justificado
                        _ => Colors.Black
                    };

                    table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignCenter().AlignMiddle()
                         .Text(status).FontColor(color).Bold().FontSize(10);
                    
                }

                // Porcentaje Final
                string percentColor = student.AttendancePercentage < 60 ? Colors.Red.Medium : Colors.Black;
                table.Cell().Background(backgroundColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignCenter().AlignMiddle()
                     .Text($"{student.AttendancePercentage}%").FontColor(percentColor).FontSize(10);
            }
        });
    }
    // --- PIE DE PÁGINA ---
    void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Leyenda: P=Presente, A=Ausente, T=Tardanza, J=Justificado").FontSize(8).FontColor(Colors.Grey.Darken2);
                col.Item().Text($"Generado el: {DateTime.Now:g}").FontSize(8).FontColor(Colors.Grey.Lighten1);
            });

            row.RelativeItem().AlignRight().Text(x =>
            {
                x.Span("Página ");
                x.CurrentPageNumber();
                x.Span(" de ");
                x.TotalPages();
            });
        });
    }
}