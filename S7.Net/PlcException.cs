namespace S7.Net;

public class PlcException : Exception
{
    public ErrorCode ErrorCode { get; }

    public PlcException(ErrorCode errorCode) : this(errorCode, $"PLC communication failed with error '{errorCode}'.")
    {
    }

    public PlcException(ErrorCode errorCode, Exception innerException) : this(errorCode, innerException.Message,
        innerException)
    {
    }

    public PlcException(ErrorCode errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public PlcException(ErrorCode errorCode, string message, Exception inner) : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}
