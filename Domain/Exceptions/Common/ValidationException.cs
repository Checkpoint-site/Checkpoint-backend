namespace Domain.Exceptions.Common;

public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, IReadOnlyDictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Ocorreram um ou mais erros de validação.")
    {
        Errors = errors;
    }
}
