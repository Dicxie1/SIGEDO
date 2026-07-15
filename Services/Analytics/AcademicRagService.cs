using Asistencia.Documents.FullReport.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
using System.Text.Json;
namespace Asistencia.Services.Analytics
{
    public class AcademicRagService
    {
        private readonly Kernel _kernel;
        
        public AcademicRagService(IConfiguration configuration)
        {
            
            string apiKey = configuration["GeminiOpenAiEndpoint:ApiKey"] ?? string.Empty;
            string modelId = configuration["GeminiOpenAiEndpoint:ModelId"] ?? "gemini-1.5-pro";
            string baseUrl = configuration["GeminiOpenAiEndpoint:BaseUrl"] ?? "http://localhost:11434/v1/";
            var customHttpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromMinutes(150)
            };
            var builder = Kernel.CreateBuilder();


            builder.AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: apiKey,
                endpoint: new Uri(baseUrl),
                httpClient : customHttpClient
                
            );

            _kernel = builder.Build();
        }
        private async Task SendStatusAsync(Stream outputStream, string stage, string message)
        {
            var statusData = new
            {
                stage = stage,
                message = message,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            };
            string json = JsonSerializer.Serialize(statusData);
            string sseEvent = $"event: status\ndata: {json}\n\n";
            byte[] bytes = Encoding.UTF8.GetBytes(sseEvent);
            await outputStream.WriteAsync(bytes, 0, bytes.Length);
            await outputStream.FlushAsync();
        }

        public async Task GenerateAcademicAnaliticsStreamAsync(FullAcademicReportViewModel data, Stream outputStream)
        {
            try
            {
                await SendStatusAsync(outputStream, "rag_retrieval", "Recuperando y analizando datos del Syllabus, Asistencia y Tutorías...");
                
                string academicContext = FormatDataToContext(data);
                var chatService = _kernel.GetRequiredService<IChatCompletionService>();

                var systemInstructions = $@"
                    Sos un Asistente de Analítica Académica Inteligente de la Universidad URACCAN. 
                    Tu objetivo es redactar un informe final  completo de una asignatura.
                    Debés basarte estrictamente en los datos proporcionados (Asistencia, Calificaciones, Avance Programático e Incidencias).
                    Usa un tono formal, académico, constructivo y humanista, alineado al Modelo del Conviene Comunitario.
                        {academicContext}
                       CRÍTICO: NO utilices nunca la sintaxis matemática de LaTeX como '$\rightarrow$' ni símbolos similares. 

                        Si necesitas representar código, variables o palabras reservadas, utiliza exclusivamente comillas invertidas de Markdown.
                        Por ejemplo:
                        - INCORRECTO: \text{{const}} o \text{{let}}
                        - CORRECTO: `const` o `let`

Si ves texto que requiera ser formateado como '\text{{texto}}', reemplázalo inmediatamente por **texto** en negrita.
                    ";
                var prompts = new string[]
                {
                    // Bloque 1: Objetivo, Actividades Académicas, Extracurriculares y Estadísticas
                    $"{systemInstructions}\n\n Genera exclusivamente las secciones en Markdown: # Objetivos del curso, # Resultados e impacto alcanzado, ## Actividades académicas (Mínimo 6 párrafos cruzando con bitácoras), ## Actividades extracurriculares y ## Estadística académica (Matrícula, Retención y Estrategias contra la deserción).",
                    
                    // Bloque 2: Logros y Desafíos
                    $"{systemInstructions}\n\n Genera exclusivamente las secciones en Markdown: # Logros (Mínimo 4 párrafos en base a calificaciones y Syllabus) y # Desafíos (## Dificultades al desarrollar la clase - 5 párrafos, y ## Desafíos que mostraron los estudiantes).",
                    
                    // Bloque 3: Perspectivas Futuras
                    $"{systemInstructions}\n\n Genera exclusivamente la sección en Markdown: # Perspectivas futuras y ## Reflexión de posibles acciones. NO incluyas anexos."
                };

                for (int i = 0; i < prompts.Length; i++)
                {
                    string stage = $"block_{i + 1}";
                    string statusMsg = i switch
                    {
                        0 => "Generando Bloque 1: Objetivos del curso, Actividades y Estadísticas Académicas...",
                        1 => "Generando Bloque 2: Análisis detallado de Logros y Desafíos académicos...",
                        2 => "Generando Bloque 3: Perspectivas futuras y plan de acción de mejora...",
                        _ => "Generando análisis del informe..."
                    };
                    await SendStatusAsync(outputStream, stage, statusMsg);

                    var history = new ChatHistory();
                    history.AddSystemMessage(systemInstructions);
                    history.AddUserMessage(prompts[i]);

                    var streamingResponse = chatService.GetStreamingChatMessageContentsAsync(history);
                    
                    Console.WriteLine($"Iniciando streaming para bloque {i + 1}...");
                    await foreach (var chunk in streamingResponse)
                    {
                        if (chunk != null && !string.IsNullOrEmpty(chunk.Content))
                        {
                            // Formato estándar Server-Sent Events (data: CONTENIDO\n\n)
                            string base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(chunk.Content));
                            string dataChunk = $"data: {base64Content}\n\n";
                            byte[] bytes = Encoding.UTF8.GetBytes(dataChunk);

                            await outputStream.WriteAsync(bytes, 0, bytes.Length);
                            await outputStream.FlushAsync(); // Fuerza el envío inmediato por la red
                        }
                    }
                }

                await SendStatusAsync(outputStream, "completed", "Generación de informe completada con éxito.");

                byte[] breakBytes = Encoding.UTF8.GetBytes($"data: {Convert.ToBase64String(Encoding.UTF8.GetBytes("\n\n"))}\n\n");
                await outputStream.WriteAsync(breakBytes, 0, breakBytes.Length);
                await outputStream.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en streaming de RAG: {ex.Message}");
                try
                {
                    await SendStatusAsync(outputStream, "error", $"Error de procesamiento: {ex.Message}");
                }
                catch
                {
                    // Ignorar si el stream ya se cerró
                }
                throw;
            }
        }
        public string FormatDataToContext(FullAcademicReportViewModel data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## 1 AVANCE PROGRAMATICO");
            foreach (var term in data.ProgrammaticProgress)
            {
                var progress = term.Value;
                sb.AppendLine($"- Parcial: {progress.TermName}");
                sb.AppendLine($"  * Matrícula Inicial: Total {progress.Initial.Total} (H: {progress.Initial.Male}, M: {progress.Initial.Female})");
                sb.AppendLine($"  * Aprobados: {progress.ApprovedPct.Total}% - Reprobados: {progress.FailedPct.Total}%");
            }
            sb.AppendLine("\n## 2. COMPORTAMIENTO DE ASISTENCIA");
            var lowAttendanceStudents = data.Attendance.Students.Where(s => s.AttendancePercentage < 80);
            sb.AppendLine($"- Total de estudiantes con asistencia menor al 80% (mínimo de ley): {lowAttendanceStudents.Count()}");
            foreach (var student in lowAttendanceStudents)
            {
                sb.AppendLine($"  * Estudiante {student.StudentId}: {student.AttendancePercentage}% de asistencia.");
            }
            sb.AppendLine("\n## 3. Actividades (Syllabus)");
            if (data.Syllabus != null && data!.Syllabus!.Any())
            {
                sb.AppendLine("| Fecha | Objetivo Específico | Unidad y Contenido | Estrategia Metodológica | Evaluación |");

                // 2. Línea de Separación Obligatoria para tablas Markdown
                sb.AppendLine("|---|---|---|---|---|");
                foreach(var syllabus  in data.Syllabus!){
                    // Limpiamos saltos de línea internos que puedan romper las filas de la tabla
                    string content = syllabus.Content?.Replace("\r\n", " ").Replace("\n", " ") ?? "N/A";
                    string strategies = syllabus.Strategies?.Replace("\r\n", " ").Replace("\n", " ") ?? "N/A";
                    string objectives = syllabus.Objectives?.Replace("\r\n", " ").Replace("\n", " ") ?? "N/A";
                    string evaluations = syllabus.Evaluations?.Replace("\r\n", " ").Replace("\n", " ") ?? "N/A";

                    sb.AppendLine($"| {syllabus.Date} | {objectives} | {content} | {strategies} | {evaluations} |");
                }
            }
            else
            {
                sb.AppendLine("- No se cargó la planificación del Syllabus para este periodo.");
            }
            sb.AppendLine("\n## 4. BITÁCORA DE ATENCIÓN ESTUDIANTIL (TUTORÍAS)");
            if (data.AttentionRecord != null && data.AttentionRecord.Any())
            {
                foreach (var record in data.AttentionRecord)
                {
                    sb.AppendLine($"- [{record.DateStr}] Categoría: {record.Category} | Prioridad: {record.Priority} | Estado: {record.Status}");
                    sb.AppendLine($"  * Observación: {record.Observation}");
                }
            }
            else
            {
                sb.AppendLine("- No se registraron incidencias ni tutorías en el periodo.");
            }
            return sb.ToString();

        }
       
    }
}
