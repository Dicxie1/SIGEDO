// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Función auxiliar para obtener token
        function getToken() {
            return document.querySelector('input[name="__RequestVerificationToken"]').value;
}
function validateStudentData(student) {
    // 1. Validar que no existan campos críticos vacíos
    if (!student.id || student.id.toString().trim() === "") return { valid: false, error: "ID requerido" };
    if (!student.name || student.name.trim() === "") return { valid: false, error: "Nombre requerido" };
    if (!student.lastName || student.lastName.trim() === "") return { valid: false, error: "Apellido requerido" };
    if (!student.cellphone || student.cellphone.trim() === "") return { valid: false, error: "Teléfono requerido" };
    if (!student.email || student.email.trim() === "")  return { valid: false, error: "Email inválido" };

    // 2. Validar formato de Correo Electrónico básico
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(student.email)) return { valid: false, error: "Email inválido" };

    // 3. Validar que Sexo corresponda a un valor válido del Enum (1 o 2)
    if (student.sexo !== 1 && student.sexo !== 2) return { valid: false, error: "Sexo inválido" };

    // 4. Validar que Etnia esté dentro del rango del Enum (1 al 7)
    if (!student.ethnic || student.ethnic < 1 || student.ethnic > 7) return { valid: false, error: "Etnia inválida" };

    return { valid: true, error: "" };
}
