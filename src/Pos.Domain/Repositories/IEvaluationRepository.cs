using Pos.Domain.Entities;

namespace Pos.Domain.Repositories;

public interface IEvaluationRepository
{
    void Add(Evaluation evaluation);
    Evaluation? Get(Guid id);
    void Update(Evaluation evaluation);
    Evaluation? Find(Guid studentId, Guid criterionId);
    IReadOnlyList<Evaluation> GetAll();
    IReadOnlyList<Evaluation> GetByStudent(Guid studentId);
    void Remove(Guid id);
    void RemoveByStudent(Guid studentId);
    void RemoveByCriterion(Guid criterionId);
}
