using Asistencia.Models.Gamification;
using Asistencia.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Asistencia.Controllers
{
    public  class QuizController : Controller
    {
        private readonly GamificationService _service;
        public QuizController(GamificationService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            List<Quiz> quizzes = await _service.GetQuizAsync();
            
            return View(quizzes);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Quiz());
        }
        [HttpPost]
        public async Task<IActionResult> Create(Quiz model)
        {
            ModelState.Remove("Id");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            foreach (var question in model.Questions)
            {
                question.AnserOptions.RemoveAll(o => string.IsNullOrWhiteSpace(o.Text));

            }
            _service.SaveQuizAsync(model);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var quiz = await _service.GetQuizAsync(id);
            if (quiz == null) return NotFound();
            string[] colors = { "#E21B3C", "#1368CE", "#D89E00", "#26890C" };
            foreach(var question in quiz.Questions)
            {
                while(question.AnserOptions.Count < 4)
                {
                    question.AnserOptions.Add(new AnwserOption { ColorCode = colors[question.AnserOptions.Count] });
                }
            }
            return View(quiz);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Quiz model)
        {
            if (id != model.QuizId) return NotFound("El ID del cuestionario no coincide.");
            ModelState.Remove("Id");
            var keysToRemove = ModelState.Keys
                .Where(k => k.Contains("QuizId") || k.Contains("Title") || k.Contains("CourseId") || k.Contains("Questions"))
                .ToList();
            foreach(var key in keysToRemove)
            {
                ModelState.Remove(key);
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var existingQuiz = await _service.GetQuizAsync(id);
            if(existingQuiz == null) return NotFound("El cuestionario no fue encontrado en la base de datos.");
            existingQuiz.Title = model.Title;
            var incomingQuestionIds = model.Questions.Where(q => q.QuestionId != 0).Select(q => q.QuestionId).ToList();
            var questionsToRemove = existingQuiz.Questions.Where(q => !incomingQuestionIds.Contains(q.QuestionId)).ToList();
            foreach (var qToRemove in questionsToRemove)
            {
                // Al removerla de la lista del padre, EF Core la eliminará de la base de datos
                // (Asumiendo que tienes Delete Behavior en Cascade)
                existingQuiz.Questions.Remove(qToRemove);
                _service.RemoveQuestion(qToRemove); // Forma explícita y más segura
            }
            if (model.Questions != null)
            {
                foreach (var incomingQuestion in model.Questions)
                {
                    if (incomingQuestion.QuestionId == 0)
                    {
                        // A. ES UNA PREGUNTA NUEVA (INSERT)
                        // Limpiamos las opciones que dejó en blanco (la 3 y la 4 usualmente)
                        if (incomingQuestion.AnserOptions != null)
                        {
                            incomingQuestion.AnserOptions.RemoveAll(o => string.IsNullOrWhiteSpace(o.Text));
                        }

                        // La agregamos a la lista del cuestionario existente
                        existingQuiz.Questions.Add(incomingQuestion);
                    }
                    else
                    {
                        // B. ES UNA PREGUNTA EXISTENTE (UPDATE)
                        var existingQuestion = existingQuiz.Questions.FirstOrDefault(q => q.QuestionId == incomingQuestion.QuestionId);

                        if (existingQuestion != null)
                        {
                            // Actualizamos sus datos básicos
                            existingQuestion.Text = incomingQuestion.Text;
                            existingQuestion.TimeLimitSecond = incomingQuestion.TimeLimitSecond;
                            existingQuestion.Point = incomingQuestion.Point;

                            // Actualizar las 4 Opciones de esta pregunta
                            if (incomingQuestion.AnserOptions != null)
                            {
                                // Usamos un bucle for tradicional para tener el índice (i) a mano
                                for (int i = 0; i < incomingQuestion.AnserOptions.Count; i++)
                                {
                                    var incomingOption = incomingQuestion.AnserOptions[i];

                                    // MAGIA: En lugar de confiar en el input oculto y el JS, leemos el Radio Button directamente.
                                    // Si el índice de este ciclo (i) coincide con el radio button seleccionado (CorrectAnswerIndex), es la correcta.
                                    bool isThisOptionCorrect = (incomingQuestion.CorrectAnswerIndex == i);

                                    // CASO A: Es una opción que ya existía en la base de datos (Tiene ID)
                                    if (incomingOption.AnwerOptionId != 0)
                                    {
                                        var existingOption = existingQuestion.AnserOptions
                                            .FirstOrDefault(o => o.AnwerOptionId == incomingOption.AnwerOptionId);

                                        if (existingOption != null)
                                        {
                                            existingOption.Text = incomingOption.Text;
                                            existingOption.IsCorrect = isThisOptionCorrect; // Guardamos el true o false
                                        }
                                    }
                                    // CASO B: Es una opción totalmente nueva (Su ID es 0)
                                    else
                                    {
                                        // Solo la agregamos a la BD si el profesor realmente le escribió algún texto
                                        if (!string.IsNullOrWhiteSpace(incomingOption.Text))
                                        {
                                            incomingOption.IsCorrect = isThisOptionCorrect;

                                            // Como es una lista conectada a Entity Framework, usar .Add() la insertará en la BD automáticamente
                                            existingQuestion.AnserOptions.Add(incomingOption);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // 7. GUARDAR TODOS LOS CAMBIOS EN UNA SOLA TRANSACCIÓN
            try
            {
                await _service.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // En caso de que algo falle a nivel de base de datos
                ModelState.AddModelError("", "No se pudieron guardar los cambios. Intente de nuevo.");
                return View(model);
            }

            // 8. REDIRIGIR AL DASHBOARD
            return RedirectToAction(nameof(Edit));
        }
        public async Task<IActionResult> Details(int id)
        {
            var detalle = await _service.GetQuizAsync(id);
            return View(detalle);
        }
    }
}
