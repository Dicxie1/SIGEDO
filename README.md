# Sistema de Gestion Docente

Una solución web integral construida con **ASP.NET Core** para la gestión de planificación académica universitaria. Este proyecto se especializa en la creación dinámica de **Syllabus (Planificaciones Didácticas)**, seguimiento de avance programático e integración con documentos externos.

## 🚀 Características Principales

### 1. Módulo de Syllabus Dinámico (Inline Editor)
Un sistema de grilla editable diseñado para alta productividad sin recargas de página.
* **Edición en Línea (Inline Editing):** Convierte celdas de texto estático en editores de texto enriquecido (**Summernote**) con un solo clic.
* **Modo "Ghost":** Los editores son invisibles y ligeros hasta que el usuario interactúa con ellos, manteniendo la interfaz limpia.
* **Gestión de Filas:** Agregar, eliminar y editar filas dinámicamente usando **Vanilla JavaScript**.
* **Guardado Individual e Híbrido:** Capacidad para guardar filas individuales (`SaveOne`) o el plan completo en lote (`SaveBatch`).

### 2. Importación Inteligente desde Word
Permite a los docentes cargar sus planificaciones existentes en formato `.docx`.
* **Parsing de Documentos:** Utiliza `DocumentFormat.OpenXml` para leer tablas dentro de documentos Word.
* **Previsualización (Preview Modal):** Los datos extraídos se muestran en una tabla modal antes de guardarse en la base de datos, permitiendo validación previa.
* **Mapeo Automático:** Asigna columnas del Word a las propiedades del modelo (Objetivos, Contenidos, Estrategias, etc.).

### 3. Reportes de Avance Programático
* Cálculo automático de estadísticas por corte evaluativo.
* Desglose demográfico (Género) de aprobados, reprobados y no examinados.
* Lógica de "Time Travel" para determinar el estado de la matrícula en fechas específicas.

---
## 🛠️ Stack Tecnológico

**Backend:**
* **Framework:** ASP.NET Core 6/8 (MVC).
* **ORM:** Entity Framework Core.
* **Base de Datos:** SQL Server.
* **Librerías Clave:** `DocumentFormat.OpenXml` (Word Processing).

**Frontend:**
* **Lenguajes:** HTML5, CSS3, **Vanilla JavaScript** (Migrado de jQuery para mejor rendimiento).
* **UI Framework:** Bootstrap 5.
* **Componentes:** * `Summernote Lite` (Rich Text Editor).
    * `SweetAlert2` (Notificaciones y modales).
    * `FontAwesome` (Iconografía).

---

## Estructura del Proyecto (Clave)

* /Controllers: Lógica de control (SyllabusController, ReportController).

* /Services: Lógica de negocio pesada (SyllabusService, ReportService). Separación de responsabilidades.

* /Views/ Vistas Razor con integración de Scripts modulares.

* /wwwroot/js: Scripts optimizados en Vanilla JS.