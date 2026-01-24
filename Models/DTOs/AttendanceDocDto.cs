using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using System.Collections.Generic;
using Asistencia.Extensions;
using System.Drawing;
namespace Asistencia.Models.DTOs
{
    public class StudentInfo
    {
        public string Code {get; set;} = string.Empty;
        public string FullName {get; set;} = string.Empty;
        public string AttendanceStatus {get; set;} = string.Empty;
        public int HoursAttended {get; set;} = 0;
        public string Note {get; set;} = string.Empty;
    }
    public class CourseAttendance
    {
        public string Career { get; set; } = string.Empty;
        public string Regimen { get; set; }  = string.Empty;
        public string Modality { get; set; }  = string.Empty;
        public int AcademicYear { get; set; }  = 0;
        public int Year { get; set; } = 0;
        public string Semester { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Campus { get; set; } = "Universidad de las regiones Autonoma de la Costa Caribe Nicaraguense";
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
    }
    public class AttendanceDocument : IDocument
    {
        private readonly CourseAttendance _courseInfo;
        private readonly List<StudentInfo> _students;

        private static IContainer CellStyle(IContainer cell)
        {
            return cell.Border(1).BorderColor(Colors.Grey.Lighten1).PaddingLeft(5).PaddingVertical(2);
        }
        public static IContainer HeaderCellStyle(IContainer cell)
        {
            return cell
            .BorderBottom(2)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingLeft(0)
            .PaddingVertical(0)
            .Background(Colors.Green.Lighten4);
        }

        public AttendanceDocument(CourseAttendance course, List<StudentInfo> students)
        {
            _courseInfo = course;
            _students = students;
        }
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(20);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }
        void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                // Fila superior: Logo y Universidad
                column.Item().Row(row =>
                {
                    row.RelativeItem(1).Height(50).AlignMiddle().AlignLeft().Image("wwwroot/img/logo.png", ImageScaling.FitHeight);
                    row.RelativeItem(3).AlignMiddle().AlignCenter().Text("UNIVERSIDAD DE LAS REGIONES AUTÓNOMAS DE COSTA CARIBE NICARAGÜENSE").SemiBold().FontSize(15);
                    row.RelativeItem(1); // Espacio vacío
                });

                column.Item().PaddingVertical(5).LineHorizontal(1);

                // Fila de información académica y docente
                column.Item().Row(row =>
                {
                    row.RelativeItem(2).Column(col =>
                    {
                        col.Item().Text($"Carrera: {_courseInfo.Career}");
                        col.Item().Text($"Régimen: {_courseInfo.Regimen}");
                        col.Item().Text($"Modalidad: {_courseInfo.Modality}");
                    });
                    row.RelativeItem(2).Column(col =>
                    {
                        col.Item().Text($"Año Lectivo: {_courseInfo.AcademicYear}");
                        col.Item().Text($"Año Académico: {_courseInfo.Year}");
                        col.Item().Text($"Semestre: {_courseInfo.Semester}");
                        col.Item().Text($"Turno: {_courseInfo.Shift}");
                    });
                    row.RelativeItem(2).Column(col =>
                    {
                        col.Item().Text($"Sección: {_courseInfo.Section}");
                        col.Item().Text($"Grado: {_courseInfo.Grade}");
                        col.Item().Text($"Recinto: {_courseInfo.Campus}");
                    });
                    var utils = new Utils();
                    var qrBytes = utils.GenerarCodigoQR(_courseInfo.CourseCode);
                    row.RelativeItem().AlignTop().AlignRight().Width(50).Height(50).Element(qr =>
                    {
                        qr.Image(qrBytes).FitWidth();
                    });
                });

                column.Item().PaddingTop(5).Column(row =>
                {
                    row.Item().Text(text =>
                    {
                        text.Span("Docente:").Bold().FontSize(10);
                        text.Span($"      {_courseInfo.TeacherName} ").FontSize(10);
                    });
                    row.Item().Row(col =>
                    {
                        col.RelativeItem(2).Text( text =>
                        {
                            text.Span("Curso:").Bold().FontSize(10);
                            text.Span($"   \t\t\t\t\t   {_courseInfo.CourseCode} - {_courseInfo.CourseName} ").FontSize(10);
                        });
                        col.ConstantItem(120).Text(text =>
                        {
                            text.Span("Fecha:").Bold().FontSize(10);
                            text.Span($"      {DateTime.Now:dd/MM/yyyy} ").FontSize(10);
                        });
                        
                    });
                });

                column.Item().PaddingVertical(5).LineHorizontal(1);
            });
        }
        void ComposeContent(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);  // #
                    columns.ConstantColumn(100);  // Código
                    columns.RelativeColumn(4);   // Nombre
                    columns.ConstantColumn(80);  // Asistencia
                    columns.ConstantColumn(60);  // Hora llegada
                    columns.RelativeColumn(1);   // Notas
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("#").SemiBold().AlignCenter();
                    header.Cell().Element(HeaderCellStyle).Text("Código").SemiBold();
                    header.Cell().Element(HeaderCellStyle).Text("Nombre Completo").SemiBold();
                    header.Cell().Element(HeaderCellStyle).Text("Asistencia").SemiBold();
                    header.Cell().Element(HeaderCellStyle).Text("Hora Llegada").SemiBold();
                    header.Cell().Element(HeaderCellStyle).Text("Notas").SemiBold();
                });

                // Rows
                int index = 1;
                foreach (var student in _students)
                {
                    table.Cell().Element(CellStyle).Text(index.ToString());
                    table.Cell().Element(CellStyle).Text(student.Code);
                    table.Cell().Element(CellStyle).Text(student.FullName);
                    table.Cell().Element(CellStyle).Text(text =>
                    {
                        text.Span("□ ").FontSize(10);
                        text.Span(" P ").FontSize(10);
                        text.Span("□ ").FontSize(10);
                        text.Span(" A ").FontSize(10);
                    });
                    table.Cell().Element(CellStyle).Text("");
                    table.Cell().Element(CellStyle).Text(student.Note ?? "");
                    index++;
                }
                table.Cell().Element(CellStyle).Text("").AlignCenter();
                table.Cell().Element(CellStyle).Text("").AlignCenter();
                table.Cell().Element(CellStyle).Text("-- Fin Listado --").AlignCenter().Italic().Bold();
                table.Cell().Element(CellStyle).Text("").AlignCenter();
                table.Cell().Element(CellStyle).Text("").AlignCenter();
                table.Cell().Element(CellStyle).Text("").AlignCenter();
                index ++;
                for(int i = index; i <= 33; i++ )
                {
                    table.Cell().Element(CellStyle).Text("");
                    table.Cell().Element(CellStyle).Text("");
                    table.Cell().Element(CellStyle).Text("");
                    table.Cell().Element(CellStyle).Text("");
                    table.Cell().Element(CellStyle).Text("");
                    table.Cell().Element(CellStyle).Text("");
                }
            });
        }
        void ComposeFooter(IContainer container)
        {
            container.PaddingTop(10).Column(col =>
            {
               col.Item().LineHorizontal(1);
               col.Spacing(5);
               col.Item().Row(row =>
               {
                  row.RelativeItem().AlignCenter().Column( col =>
                  {
                     col.Item().Height(10);
                     col.Item().LineHorizontal(1);
                     col.Item().Text("Docente : Dicxie Danuard Madrigal Brack").FontSize(8);
                     col.Item().AlignCenter().Text("Firma").FontSize(8);
                  });
               }); 
            });
        }
    }
}
