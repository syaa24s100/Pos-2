using Pos.Domain.Entities;
using Pos.Domain.Repositories;

namespace Pos.Infrastructure;

public sealed class InMemoryEvaluationRepository : IEvaluationRepository
{
    private readonly List<Evaluation> _items = new();

    public void Add(Evaluation evaluation) => _items.Add(evaluation);
    public Evaluation? Get(Guid id) => _items.FirstOrDefault(e => e.Id == id);
    public void Update(Evaluation evaluation) { } // in-memory: изменения уже в объекте
    public Evaluation? Find(Guid studentId, Guid criterionId) =>
        _items.FirstOrDefault(e => e.StudentId == studentId && e.CriterionId == criterionId);
    public IReadOnlyList<Evaluation> GetAll() => _items.ToList();
    public IReadOnlyList<Evaluation> GetByStudent(Guid studentId) =>
        _items.Where(e => e.StudentId == studentId).ToList();
    public void Remove(Guid id) => _items.RemoveAll(e => e.Id == id);
    public void RemoveByStudent(Guid studentId) => _items.RemoveAll(e => e.StudentId == studentId);
    public void RemoveByCriterion(Guid criterionId) => _items.RemoveAll(e => e.CriterionId == criterionId);
}
