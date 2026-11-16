using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.Services;
using Pos.Domain.Strategies;
using Xunit;

namespace Pos.Tests;

public class FinalGradeTests
{
    private readonly FinalGradeCalculator _calc = new();

    private static (Rubric rubric, Criterion hw, Criterion exam) Setup()
    {
        var rubric = new Rubric();
        return (rubric, rubric.AddCriterion("ДЗ", 10, 0.4m), rubric.AddCriterion("Контрольная", 20, 0.6m));
    }

    [Fact]
    public void All_strategy_counts_missing_criterion_as_zero()
    {
        var (rubric, hw, _) = Setup();
        var student = Guid.NewGuid();
        var evals = new[] { EvaluationFactory.Create(student, rubric, hw.Id, 10, DateTime.Now) };

        var grade = _calc.Calculate(rubric, evals, new WeightedAllCriteriaStrategy());

        Assert.Equal(40m, grade.Percent); // 0.4 * 100% + 0.6 * 0
    }

    [Fact]
    public void Rated_strategy_ignores_missing_criterion()
    {
        var (rubric, hw, _) = Setup();
        var student = Guid.NewGuid();
        var evals = new[] { EvaluationFactory.Create(student, rubric, hw.Id, 10, DateTime.Now) };

        var grade = _calc.Calculate(rubric, evals, new WeightedRatedOnlyStrategy());

        Assert.Equal(100m, grade.Percent);
    }

    [Fact]
    public void Final_grade_follows_current_evaluations()
    {
        var (rubric, hw, exam) = Setup();
        var student = Guid.NewGuid();
        var e1 = EvaluationFactory.Create(student, rubric, hw.Id, 10, DateTime.Now);
        var e2 = EvaluationFactory.Create(student, rubric, exam.Id, 10, DateTime.Now);
        var strategy = new WeightedAllCriteriaStrategy();

        Assert.Equal(70m, _calc.Calculate(rubric, new[] { e1, e2 }, strategy).Percent); // 40 + 0.6*50

        e2.Correct(exam, new(20), DateTime.Now, null);

        Assert.Equal(100m, _calc.Calculate(rubric, new[] { e1, e2 }, strategy).Percent);
    }

    [Fact]
    public void No_evaluations_gives_zero()
    {
        var (rubric, _, _) = Setup();
        var grade = _calc.Calculate(rubric, Array.Empty<Evaluation>(), new WeightedAllCriteriaStrategy());
        Assert.Equal(0m, grade.Percent);
        Assert.Equal(0m, grade.Mark10);
    }
}
