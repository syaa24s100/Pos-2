using Pos.Application.Dtos;
using Pos.Application.Ports;
using Pos.Domain.Entities;
using Pos.Domain.Factories;
using Pos.Domain.Repositories;
using Pos.Domain.Services;
using Pos.Domain.Strategies;
using Pos.Domain.ValueObjects;

namespace Pos.Application;

public sealed class PosFacade
{
    private readonly IStudentRepository _students;
    private readonly IRubricRepository _rubrics;
    private readonly IEvaluationRepository _evaluations;
    private readonly IResultExporter _exporter;
    private readonly FinalGradeCalculator _calculator;
    private readonly Dictionary<string, IFinalGradeStrategy> _strategies;
    private IFinalGradeStrategy _current;

    public PosFacade(
        IStudentRepository students,
        IRubricRepository rubrics,
        IEvaluationRepository evaluations,
        IResultExporter exporter,
        FinalGradeCalculator calculator,
        IEnumerable<IFinalGradeStrategy> strategies)
    {
        _students = students;
        _rubrics = rubrics;
        _evaluations = evaluations;
        _exporter = exporter;
        _calculator = calculator;
        _strategies = strategies.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
        _current = _strategies.Values.First();
    }


    public StudentDto AddStudent(string fullName)
    {
        var student = new Student(fullName);
        _students.Add(student);
        return new StudentDto(student.Id, student.FullName);
    }

    public IReadOnlyList<StudentDto> ListStudents() =>
        _students.GetAll().Select(s => new StudentDto(s.Id, s.FullName)).ToList();

    public StudentDto RenameStudent(Guid studentId, string fullName)
    {
        var student = GetStudent(studentId);
        student.Rename(fullName);
        _students.Update(student);
        return new StudentDto(student.Id, student.FullName);
    }

    public void RemoveStudent(Guid studentId)
    {
        GetStudent(studentId);
        _evaluations.RemoveByStudent(studentId);
        _students.Remove(studentId);
    }


    public CriterionDto AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var rubric = _rubrics.Get();
        var c = rubric.AddCriterion(name, maxScore, weight);
        _rubrics.Save(rubric);
        return ToDto(c);
    }

    public IReadOnlyList<CriterionDto> ListCriteria() =>
        _rubrics.Get().Criteria.Select(c => ToDto(c)).ToList();

    public void RemoveCriterion(Guid criterionId)
    {
        var rubric = _rubrics.Get();
        rubric.RemoveCriterion(criterionId);
        _rubrics.Save(rubric);
        _evaluations.RemoveByCriterion(criterionId); 
    }



    public EvaluationDto IssueGrade(Guid studentId, Guid criterionId, decimal score, string? comment = null)
    {
        var student = GetStudent(studentId);
        if (_evaluations.Find(studentId, criterionId) is not null)
            throw new UseCaseException("Оценка по этому критерию уже выставлена — воспользуйтесь исправлением.");

        var rubric = _rubrics.Get();
        var evaluation = EvaluationFactory.Create(studentId, rubric, criterionId, score, DateTime.Now, comment);
        _evaluations.Add(evaluation);
        return ToDto(evaluation, student.FullName, rubric.GetCriterion(criterionId));
    }

    public IReadOnlyList<EvaluationDto> ListGrades(Guid? studentId = null)
    {
        var rubric = _rubrics.Get();
        var source = studentId is null ? _evaluations.GetAll() : _evaluations.GetByStudent(studentId.Value);

        var result = new List<EvaluationDto>();
        foreach (var e in source)
        {
            var student = _students.Get(e.StudentId);
            if (student is null || !rubric.Contains(e.CriterionId)) continue;
            result.Add(ToDto(e, student.FullName, rubric.GetCriterion(e.CriterionId)));
        }
        return result;
    }

    public EvaluationDto CorrectGrade(Guid evaluationId, decimal newScore, string? comment = null)
    {
        var evaluation = _evaluations.Get(evaluationId)
            ?? throw new UseCaseException("Оценка не найдена.");
        var rubric = _rubrics.Get();
        var criterion = rubric.GetCriterion(evaluation.CriterionId);

        evaluation.Correct(criterion, new Score(newScore), DateTime.Now, comment);
        _evaluations.Update(evaluation);
        return ToDto(evaluation, GetStudent(evaluation.StudentId).FullName, criterion);
    }

    public void CancelGrade(Guid evaluationId)
    {
        if (_evaluations.Get(evaluationId) is null)
            throw new UseCaseException("Оценка не найдена.");
        _evaluations.Remove(evaluationId);
    }

    

    public IReadOnlyList<string> AvailableStrategies =>
        _strategies.Values.Select(s => $"{s.Name} — {s.Description}").ToList();

    public string CurrentStrategy => $"{_current.Name} — {_current.Description}";

    public void UseStrategy(string name)
    {
        if (!_strategies.TryGetValue(name.Trim(), out var strategy))
            throw new UseCaseException($"Правило «{name}» не найдено.");
        _current = strategy;
    }

    public ResultDto CalculateFinal(Guid studentId)
    {
        var student = GetStudent(studentId);
        var rubric = _rubrics.Get();
        var evaluations = _evaluations.GetByStudent(studentId);

        var grade = _calculator.Calculate(rubric, evaluations, _current);
        var rated = evaluations.Count(e => rubric.Contains(e.CriterionId));
        return new ResultDto(student.Id, student.FullName, grade.Percent, grade.Mark10,
            rated, rubric.Criteria.Count, _current.Name);
    }

    public IReadOnlyList<ResultDto> CalculateAll() =>
        _students.GetAll().Select(s => CalculateFinal(s.Id)).ToList();

    public int ExportResults(string path)
    {
        var results = CalculateAll();
        _exporter.Export(results, path);
        return results.Count;
    }


    private Student GetStudent(Guid id) =>
        _students.Get(id) ?? throw new UseCaseException("Студент не найден.");

    private static CriterionDto ToDto(Criterion c) => new(c.Id, c.Name, c.MaxScore, c.Weight);

    private static EvaluationDto ToDto(Evaluation e, string studentName, Criterion c) =>
        new(e.Id, e.StudentId, studentName, c.Id, c.Name, e.Score.Value, c.MaxScore, e.Date, e.Comment);
}
