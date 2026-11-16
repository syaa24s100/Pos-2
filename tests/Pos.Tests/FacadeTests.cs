using Pos.Application;
using Pos.Application.Dtos;
using Pos.Application.Ports;
using Pos.Domain.Common;
using Pos.Domain.Services;
using Pos.Domain.Strategies;
using Pos.Infrastructure;
using Xunit;

namespace Pos.Tests;

public class FacadeTests
{
    private sealed class NullExporter : IResultExporter
    {
        public void Export(IEnumerable<ResultDto> results, string path) { }
    }

    private static PosFacade CreateFacade() => new(
        new InMemoryStudentRepository(),
        new InMemoryRubricRepository(),
        new InMemoryEvaluationRepository(),
        new NullExporter(),
        new FinalGradeCalculator(),
        new IFinalGradeStrategy[] { new WeightedAllCriteriaStrategy(), new WeightedRatedOnlyStrategy() });

    [Fact]
    public void Issue_grade_out_of_range_is_rejected()
    {
        var pos = CreateFacade();
        var student = pos.AddStudent("Иванов");
        var criterion = pos.AddCriterion("ДЗ", 10, 1);

        Assert.Throws<DomainException>(() => pos.IssueGrade(student.Id, criterion.Id, 12));
        Assert.Empty(pos.ListGrades());
    }

    [Fact]
    public void Issue_grade_for_unknown_criterion_is_rejected()
    {
        var pos = CreateFacade();
        var student = pos.AddStudent("Иванов");

        Assert.Throws<DomainException>(() => pos.IssueGrade(student.Id, Guid.NewGuid(), 5));
    }

    [Fact]
    public void Duplicate_grade_is_rejected_but_can_be_corrected_and_cancelled()
    {
        var pos = CreateFacade();
        var student = pos.AddStudent("Иванов");
        var criterion = pos.AddCriterion("ДЗ", 10, 1);
        var grade = pos.IssueGrade(student.Id, criterion.Id, 5);

        Assert.Throws<UseCaseException>(() => pos.IssueGrade(student.Id, criterion.Id, 6));

        pos.CorrectGrade(grade.Id, 8);
        Assert.Equal(8, pos.ListGrades().Single().Score);

        pos.CancelGrade(grade.Id);
        Assert.Empty(pos.ListGrades());
    }

    [Fact]
    public void Removing_criterion_removes_its_grades()
    {
        var pos = CreateFacade();
        var student = pos.AddStudent("Иванов");
        var criterion = pos.AddCriterion("ДЗ", 10, 1);
        pos.IssueGrade(student.Id, criterion.Id, 5);

        pos.RemoveCriterion(criterion.Id);

        Assert.Empty(pos.ListGrades());
        Assert.Equal(0m, pos.CalculateFinal(student.Id).Percent);
    }

    [Fact]
    public void Strategy_can_be_switched()
    {
        var pos = CreateFacade();
        var student = pos.AddStudent("Иванов");
        var hw = pos.AddCriterion("ДЗ", 10, 1);
        pos.AddCriterion("Экзамен", 10, 1);
        pos.IssueGrade(student.Id, hw.Id, 10);

        pos.UseStrategy("all");
        Assert.Equal(50m, pos.CalculateFinal(student.Id).Percent);

        pos.UseStrategy("rated");
        Assert.Equal(100m, pos.CalculateFinal(student.Id).Percent);
    }

    [Fact]
    public void Student_can_be_renamed_but_not_to_empty_name()
    {
        var pos = CreateFacade();
        var student = pos.AddStudent("Иванов");

        pos.RenameStudent(student.Id, "Иванов Иван");
        Assert.Equal("Иванов Иван", pos.ListStudents().Single().FullName);

        Assert.Throws<DomainException>(() => pos.RenameStudent(student.Id, " "));
    }

    [Fact]
    public void Correction_without_comment_keeps_previous_comment()
    {
        var pos = CreateFacade();
        var student = pos.AddStudent("Иванов");
        var criterion = pos.AddCriterion("ДЗ", 10, 1);
        var grade = pos.IssueGrade(student.Id, criterion.Id, 5, "сдал с опозданием");

        pos.CorrectGrade(grade.Id, 6);

        Assert.Equal("сдал с опозданием", pos.ListGrades().Single().Comment);
    }
}
