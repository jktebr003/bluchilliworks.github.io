using System.Runtime.Serialization;

namespace Web.Shared.Exceptions;

public class HttpPostException : Exception
{
    public HttpPostException()
    {
    }

    public HttpPostException(string message)
        : base(message)
    {
    }

    public HttpPostException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected HttpPostException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
