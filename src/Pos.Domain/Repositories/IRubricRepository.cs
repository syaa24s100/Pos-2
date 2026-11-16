using Pos.Domain.Entities;

namespace Pos.Domain.Repositories;

public interface IRubricRepository
{
    Rubric Get();
    void Save(Rubric rubric);
}
