using Pos.Domain.Entities;
using Pos.Domain.Repositories;

namespace Pos.Infrastructure;

public sealed class InMemoryRubricRepository : IRubricRepository
{
    private readonly Rubric _rubric = new();

    public Rubric Get() => _rubric;
    public void Save(Rubric rubric) { } // in-memory: агрегат хранится по ссылке
}
