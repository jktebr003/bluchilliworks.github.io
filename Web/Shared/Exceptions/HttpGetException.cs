using System.Runtime.Serialization;

namespace Web.Shared.Exceptions;

public class HttpGetException : Exception
{
    public HttpGetException()
    {
    }

    public HttpGetException(string message)
        : base(message)
    {
    }

    public HttpGetException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected HttpGetException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
