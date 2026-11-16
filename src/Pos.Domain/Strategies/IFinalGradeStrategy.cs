using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Strategies;

public interface IFinalGradeStrategy
{
    string Name { get; }
    string Description { get; }

    decimal CalculatePercent(IReadOnlyList<Criterion> criteria, IReadOnlyDictionary<Guid, Score> scores);
}
