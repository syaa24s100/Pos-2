using Pos.Domain.Common;
using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.ValueObjects;
using Xunit;

namespace Pos.Tests;

public class InvariantTests
{
    private static (Rubric rubric, Criterion criterion) RubricWithCriterion(decimal max = 10, decimal weight = 1)
    {
        var rubric = new Rubric();
        return (rubric, rubric.AddCriterion("Домашки", max, weight));
    }

    [Fact]
    public void Score_cannot_be_negative() =>
        Assert.Throws<DomainException>(() => new Score(-1));

    [Fact]
    public void Score_is_compared_by_value() =>
        Assert.Equal(new Score(5), new Score(5));

    [Fact]
    public void Factory_rejects_score_above_criterion_max()
    {
        var (rubric, criterion) = RubricWithCriterion(10);
        Assert.Throws<DomainException>(() =>
            EvaluationFactory.Create(Guid.NewGuid(), rubric, criterion.Id, 12, DateTime.Now));
    }

    [Fact]
    public void Factory_accepts_score_equal_to_max()
    {
        var (rubric, criterion) = RubricWithCriterion(10);
        var e = EvaluationFactory.Create(Guid.NewGuid(), rubric, criterion.Id, 10, DateTime.Now);
        Assert.Equal(10, e.Score.Value);
    }

    [Fact]
    public void Factory_rejects_criterion_not_in_rubric()
    {
        var rubric = new Rubric();
        Assert.Throws<DomainException>(() =>
            EvaluationFactory.Create(Guid.NewGuid(), rubric, Guid.NewGuid(), 5, DateTime.Now));
    }

    [Fact]
    public void Correct_keeps_range_invariant()
    {
        var (rubric, criterion) = RubricWithCriterion(10);
        var e = EvaluationFactory.Create(Guid.NewGuid(), rubric, criterion.Id, 5, DateTime.Now);

        Assert.Throws<DomainException>(() => e.Correct(criterion, new Score(11), DateTime.Now, null));
        e.Correct(criterion, new Score(7), DateTime.Now, "пересчитано");
        Assert.Equal(7, e.Score.Value);
    }

    [Fact]
    public void Rubric_rejects_duplicate_names_and_bad_values()
    {
        var rubric = new Rubric();
        rubric.AddCriterion("Экзамен", 10, 1);
        Assert.Throws<DomainException>(() => rubric.AddCriterion("экзамен", 10, 1));
        Assert.Throws<DomainException>(() => rubric.AddCriterion("A", 0, 1));
        Assert.Throws<DomainException>(() => rubric.AddCriterion("B", 10, 0));
    }

    [Fact]
    public void Student_name_cannot_be_empty() =>
        Assert.Throws<DomainException>(() => new Student("  "));
}
