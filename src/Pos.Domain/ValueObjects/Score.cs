using Pos.Domain.Common;

namespace Pos.Domain.ValueObjects;


public sealed record Score
{
    public decimal Value { get; }

    public Score(decimal value)
    {
        if (value < 0)
            throw new DomainException("Балл не может быть отрицательным.");
        Value = value;
    }
}
