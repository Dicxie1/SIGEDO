# SIGEDO - Guía de Contexto para Gemini

Este documento define los estándares, arquitectura y convenciones del proyecto **SIGEDO** (Sistema de Gestión Docente). Gemini debe seguir estas directrices estrictamente.

## 🛠 Stack Tecnológico
- **Framework:** ASP.NET Core MVC (.NET 8.0+)
- **ORM:** Entity Framework Core (Code First)
- **Base de Datos:** PosgreSQL (configurado vía `ApplicationDbContext`)
- **Frontend:** Razor Views (.cshtml), JavaScript (Vanilla/jQuery), CSS Personalizado
- **Tiempo Real:** SignalR (vía `QuizHub`)
- **Reportes:** Excel (vía `ExcelGradebookExportService`)

## 🏗 Arquitectura y Patrones
El proyecto sigue una arquitectura multicapa dentro del mismo ensamblado:
1.  **Controllers:** Solo gestionan el flujo de las peticiones. No contienen lógica de negocio compleja.
2.  **Services:** Contienen la lógica de negocio. Se inyectan vía DI en los controladores.
3.  **Models (Entidades):** Representan las tablas de la DB.
4.  **DTOs:** Utilizados para transferencia de datos entre capas o peticiones API.
5.  **ViewModels:** Modelos específicos para las vistas Razor.
6.  **Configurations:** Configuración de Fluent API para EF Core en `Data/Configurations`.

## 🎨 Convenciones de Código
- **Lenguaje:** C# con PascalCase para clases y métodos, camelCase para variables locales y campos privados con guion bajo (e.g., `_context`).
- **Inyección de Dependencias:** Preferir inyección por constructor.
- **Asincronía:** Usar `async/await` en toda la cadena (desde Controller hasta DB) siempre que sea posible.
- **Validación:** Usar Data Annotations en ViewModels/DTOs y lógica de validación en Services.

## 📜 Reglas Específicas del Proyecto
- **Entidades:** No exponer entidades de base de datos directamente en las vistas; usar `ViewModels`.
- **Contexto de Datos:** El `ApplicationDbContext` debe ser usado preferentemente dentro de los `Services`.
- **Naming:** 
    - Servicios: `[Name]Service` e interfaces `I[Name]Service`.
    - DTOs: `[Action][Entity]Dto` (e.g., `CreateCourseDto`).
    - Vistas: Deben estar en su carpeta correspondiente dentro de `Views/[ControllerName]`.

## 🧪 Pruebas y Validación
- Antes de dar por finalizada una tarea, verificar que el proyecto compile con `dotnet build`.
- Si se modifican modelos, generar la migración correspondiente con `dotnet ef migrations add`.
