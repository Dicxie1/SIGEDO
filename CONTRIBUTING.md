# Guía de Contribución

¡Gracias por tu interés en contribuir a este proyecto! Este documento describe cómo puedes participar de manera efectiva, siguiendo la estrategia de **Trunk-Based Development (TBD)**.

---

## **1. Filosofía de Trunk-Based Development**
En este proyecto, seguimos **Trunk-Based Development**, lo que significa:
- **Trabajamos directamente en la rama principal (`main`)**.
- **Evitar ramas de larga duración**: Las ramas temporales deben ser cortas (horas o días, no semanas).
- **Commits pequeños y frecuentes**: Cada commit debe ser funcional y pasar todas las pruebas.
- **Integración continua**: Los cambios se integran y prueban automáticamente en la rama principal.

---

## **2. Requisitos Previos**
Antes de contribuir, asegúrate de:
- Tener instalado **Git** y configurado correctamente.
- Conocer los conceptos básicos de **TypeScript/SCSS** (según el proyecto).
- Familiarizarte con las herramientas de **pruebas y linting** del proyecto.

---

## **3. Flujo de Trabajo para Contribuir**

### **3.1. Clonar el Repositorio**
```bash
git clone <URL_DEL_REPOSITORIO>
cd <NOMBRE_DEL_PROYECTO>
```
### **3.2. Configurar el Entorno** 
Restaura las dependencias de NuGet:
```bash
dotnet restore
```
Ejecuta el proyecto localmente para verificar que todo funcione:
```bash
dotnet run
```

### **3.3. Crear una Rama Temporal**
Si trabajas en una característica pequeña o un bugfix, puedes hacerlo directamente en main. Si necesitas aislar cambios, crea una rama corta

```bash
git checkout -b feature/<nombre-corto>
```

### **3.4. Realizar Cambios** 

* Commits pequeños y descriptivos:
```bash
git commit -m "feat: añadir controlador para gestión de usuarios"
```
Usa el formato:

* feat: para nuevas características.
* fix: para correcciones de bugs.
* docs: para cambios en documentación.
* refactor: para mejoras de código sin cambiar funcionalidad.

#### * Ejecución prueba local
```bash
dotnet test
```

## **4. Revisión de código**
* No se requieren Pull Requests en TBD puro, pero si el equipo lo prefiere, se pueden usar para revisiones rápidas.
* Los commits deben ser autoexplicativos y pasar todas las pruebas automatizadas.

## **5. Convenciones de Código**
 ```text
 Pendiente agregar
 ```

 ---

## **6. Pruebas**

* Pruebas unitarias: Usa xUnit, NUnit, o MSTest para probar lógica de negocio y controladores.
* Pruebas de integración: Prueba flujos completos, como rutas y vistas.

---




