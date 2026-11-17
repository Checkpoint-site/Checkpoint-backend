namespace Domain.Exceptions.Common;
public class ValueErrorException : Exception
{
    public ValueErrorException(string message) : base(message)
    {
    }
}

