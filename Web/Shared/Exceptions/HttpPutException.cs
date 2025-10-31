using System.Runtime.Serialization;

namespace Web.Shared.Exceptions;

public class HttpPutException : Exception
{
    public HttpPutException()
    {
    }

    public HttpPutException(string message)
        : base(message)
    {
    }

    public HttpPutException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected HttpPutException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
