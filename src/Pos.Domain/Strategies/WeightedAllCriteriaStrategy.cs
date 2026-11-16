using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Strategies;

public sealed class WeightedAllCriteriaStrategy : IFinalGradeStrategy
{
    public string Name => "all";
    public string Description => "по всем критериям рубрикатора (не выставленная оценка = 0)";

    public decimal CalculatePercent(IReadOnlyList<Criterion> criteria, IReadOnlyDictionary<Guid, Score> scores)
    {
        var totalWeight = criteria.Sum(c => c.Weight);
        if (totalWeight == 0) return 0m;

        var sum = criteria.Sum(c =>
            scores.TryGetValue(c.Id, out var s) ? c.Weight * s.Value / c.MaxScore : 0m);

        return Math.Round(sum / totalWeight * 100m, 2);
    }
}
