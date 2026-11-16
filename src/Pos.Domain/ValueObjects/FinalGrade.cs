using Pos.Domain.Common;

namespace Pos.Domain.ValueObjects;

public sealed record FinalGrade
{
    public decimal Percent { get; }
    public decimal Mark10 => Math.Round(Percent / 10m, 1);

    public FinalGrade(decimal percent)
    {
        if (percent < 0 || percent > 100)
            throw new DomainException("Итог должен быть в диапазоне 0..100%.");
        Percent = percent;
    }
}
