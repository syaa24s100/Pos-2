using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Strategies;

public sealed class WeightedRatedOnlyStrategy : IFinalGradeStrategy
{
    public string Name => "rated";
    public string Description => "только по уже оценённым критериям (промежуточный итог)";

    public decimal CalculatePercent(IReadOnlyList<Criterion> criteria, IReadOnlyDictionary<Guid, Score> scores)
    {
        var rated = criteria.Where(c => scores.ContainsKey(c.Id)).ToList();
        var totalWeight = rated.Sum(c => c.Weight);
        if (totalWeight == 0) return 0m;

        var sum = rated.Sum(c => c.Weight * scores[c.Id].Value / c.MaxScore);
        return Math.Round(sum / totalWeight * 100m, 2);
    }
}
