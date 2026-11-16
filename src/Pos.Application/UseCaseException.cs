namespace Pos.Application;
public sealed class UseCaseException : Exception
{
    public UseCaseException(string message) : base(message) { }
}
