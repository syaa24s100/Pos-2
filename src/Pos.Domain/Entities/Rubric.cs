using Pos.Domain.Common;

namespace Pos.Domain.Entities;


public sealed class Rubric
{
    private readonly List<Criterion> _criteria = new();

    public IReadOnlyList<Criterion> Criteria => _criteria;

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (_criteria.Any(c => string.Equals(c.Name, trimmed, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Критерий «{trimmed}» уже есть в рубрикаторе.");

        var criterion = new Criterion(trimmed, maxScore, weight);
        _criteria.Add(criterion);
        return criterion;
    }

    public bool Contains(Guid criterionId) => _criteria.Any(c => c.Id == criterionId);

    public Criterion GetCriterion(Guid criterionId) =>
        _criteria.FirstOrDefault(c => c.Id == criterionId)
        ?? throw new DomainException("Критерия нет в рубрикаторе.");

    public void RemoveCriterion(Guid criterionId) => _criteria.Remove(GetCriterion(criterionId));
}
