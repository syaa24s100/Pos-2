using Pos.Domain.Entities;
using Pos.Domain.Strategies;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Services;

public sealed class FinalGradeCalculator
{
    public FinalGrade Calculate(Rubric rubric, IEnumerable<Evaluation> evaluations, IFinalGradeStrategy strategy)
    {
        var scores = new Dictionary<Guid, Score>();
        foreach (var e in evaluations)
            if (rubric.Contains(e.CriterionId))
                scores[e.CriterionId] = e.Score;

        var percent = strategy.CalculatePercent(rubric.Criteria, scores);
        return new FinalGrade(percent);
    }
}
