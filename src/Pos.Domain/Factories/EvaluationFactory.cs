using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Factories;

public static class EvaluationFactory
{
    public static Evaluation Create(
        Guid studentId, Rubric rubric, Guid criterionId,
        decimal score, DateTime date, string? comment = null)
    {
        var criterion = rubric.GetCriterion(criterionId); 
        return new Evaluation(studentId, criterion, new Score(score), date, comment);
    }
}
