using Pos.Domain.Entities;
using Pos.Domain.Repositories;

namespace Pos.Infrastructure;

public sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _items = new();

    public void Add(Student student) => _items.Add(student);
    public Student? Get(Guid id) => _items.FirstOrDefault(s => s.Id == id);
    public void Update(Student student) { } 
    public IReadOnlyList<Student> GetAll() => _items.ToList();
    public void Remove(Guid id) => _items.RemoveAll(s => s.Id == id);
}
