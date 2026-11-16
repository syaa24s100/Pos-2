using Pos.Domain.Entities;

namespace Pos.Domain.Repositories;

public interface IStudentRepository
{
    void Add(Student student);
    Student? Get(Guid id);
    void Update(Student student);
    IReadOnlyList<Student> GetAll();
    void Remove(Guid id);
}
