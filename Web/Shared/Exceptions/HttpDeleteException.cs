using System.Runtime.Serialization;

namespace Web.Shared.Exceptions;

public class HttpDeleteException : Exception
{
    public HttpDeleteException()
    {
    }

    public HttpDeleteException(string message)
        : base(message)
    {
    }

    public HttpDeleteException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected HttpDeleteException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
