namespace Pos.Application.Dtos;

public sealed record StudentDto(Guid Id, string FullName);

public sealed record CriterionDto(Guid Id, string Name, decimal MaxScore, decimal Weight);

public sealed record EvaluationDto(
    Guid Id, Guid StudentId, string StudentName,
    Guid CriterionId, string CriterionName,
    decimal Score, decimal MaxScore, DateTime Date, string? Comment);

public sealed record ResultDto(
    Guid StudentId, string StudentName,
    decimal Percent, decimal Mark10,
    int RatedCriteria, int TotalCriteria, string Strategy);
