using Pos.Domain.Common;

namespace Pos.Domain.Entities;

public sealed class Student
{
    public Guid Id { get; }
    public string FullName { get; private set; }

    public Student(string fullName)
    {
        Id = Guid.NewGuid();
        FullName = Validate(fullName);
    }

    public void Rename(string fullName) => FullName = Validate(fullName);

    private static string Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Имя студента не может быть пустым.");
        return name.Trim();
    }
}
