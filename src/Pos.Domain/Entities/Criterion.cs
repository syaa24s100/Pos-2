using Pos.Domain.Common;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Entities;


public sealed class Criterion
{
    public Guid Id { get; }
    public string Name { get; }
    public decimal MaxScore { get; }
    public decimal Weight { get; }

    internal Criterion(string name, decimal maxScore, decimal weight)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Название критерия не может быть пустым.");
        if (maxScore <= 0)
            throw new DomainException("Максимальный балл критерия должен быть больше 0.");
        if (weight <= 0)
            throw new DomainException("Вес критерия должен быть больше 0.");

        Id = Guid.NewGuid();
        Name = name.Trim();
        MaxScore = maxScore;
        Weight = weight;
    }

    public bool Accepts(Score score) => score.Value <= MaxScore;

    public void EnsureAccepts(Score score)
    {
        if (!Accepts(score))
            throw new DomainException(
                $"Балл {score.Value} превышает максимум {MaxScore} по критерию «{Name}».");
    }
}
