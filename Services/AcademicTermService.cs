using Asistencia.Data;
using Asistencia.Models;
using Asistencia.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asistencia.Services
{
    
    public class AcademicTermService
    {
        private readonly ApplicationDbContext _context;
        public AcademicTermService(ApplicationDbContext context)
        {
            _context = context;
        }
        async public Task<EvaluationConfigViewModel>? GetCourseTerm(int courseid)
        {
            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.AcademicTerms)
                .ThenInclude(a => a.Assignments)
                .Where(c => c.IdCourse == courseid).FirstOrDefaultAsync();
            if (course == null) return null;
            var termViewModel = new EvaluationConfigViewModel
            {
                CourseId = course.IdCourse,
                CourseCode = course?.Subject?.SubjectId ?? "N/A",
                CourseName = course?.Subject?.SubjetName ?? "N/A",
                Terms = course?.AcademicTerms.ToList() ?? new List<AcademicTerm>()
            };
            return termViewModel;
        }
        async public void SaveAcademicTerm( AcademicTerm model)
        {
            _context.Add(model);
            await _context.SaveChangesAsync();
        }
        async public Task<AcademicTerm?> GetAcademcTermAsync(int courseId, int termId)
        {
            return await _context.AcademicTerms
                .Where(a => a.CourseId == courseId && a.TermId == termId).FirstOrDefaultAsync();
        }
        async public Task<string> UpdateAcademicTerm(AcademicTerm model)
        {
            if (model.StartDate > model.EndDate)
                return "DateInvalid";
            if (model.AccumulatedWeight + model.ExamWeight != 100)
                return "SumGrateThan100";
            try
            {
                var term = await _context.AcademicTerms
                    .FirstOrDefaultAsync(t => t.TermId == model.TermId && t.CourseId == model.CourseId);
                if (term == null)
                    return "NoExist";

                var otherTermWirght = await _context.AcademicTerms
                    .Where(t => t.CourseId == model.CourseId && t.TermId != model.TermId )
                    .SumAsync(t => t.WeightOnFinalGrade);
                if(otherTermWirght + model.WeightOnFinalGrade > 100)
                {
                    var avaible = 100 - otherTermWirght;
                    return "MoreTha100";
                }
                term.Name = model.Name;
                term.WeightOnFinalGrade = model.WeightOnFinalGrade;
                term.AccumulatedWeight = model.AccumulatedWeight;
                term.ExamWeight = model.ExamWeight;
                term.StartDate = model.StartDate;
                term.EndDate = model.EndDate;
                _context.AcademicTerms.Update(term);
                await _context.SaveChangesAsync();
                return "succes";
            }catch(Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
