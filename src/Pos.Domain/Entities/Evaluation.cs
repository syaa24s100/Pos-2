using Pos.Domain.Common;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Entities;


public sealed class Evaluation
{
    public Guid Id { get; }
    public Guid StudentId { get; }
    public Guid CriterionId { get; }
    public Score Score { get; private set; }
    public DateTime Date { get; private set; }
    public string? Comment { get; private set; }

    internal Evaluation(Guid studentId, Criterion criterion, Score score, DateTime date, string? comment)
    {
        criterion.EnsureAccepts(score);

        Id = Guid.NewGuid();
        StudentId = studentId;
        CriterionId = criterion.Id;
        Score = score;
        Date = date;
        Comment = comment;
    }

    /// <summary>Исправление оценки — снова с проверкой диапазона критерия.</summary>
    public void Correct(Criterion criterion, Score newScore, DateTime date, string? comment)
    {
        if (criterion.Id != CriterionId)
            throw new DomainException("Оценка относится к другому критерию.");
        criterion.EnsureAccepts(newScore);

        Score = newScore;
        Date = date;
        Comment = comment ?? Comment;
    }
}
