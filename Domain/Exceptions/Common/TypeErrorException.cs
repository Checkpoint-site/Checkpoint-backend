namespace Domain.Exceptions.Common;
public class TypeErrorException : Exception
{
    public TypeErrorException(string message) : base(message)
    {
    }
}

