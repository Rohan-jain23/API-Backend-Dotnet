using System;
using WuH.Ruby.Common.Core;

namespace FrameworkAPI.Exceptions;

public class InternalServiceException : Exception
{
    public int StatusCode { get; }

    public InternalServiceException()
    {
    }

    public InternalServiceException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public InternalServiceException(InternalError internalError)
        : base(internalError.ErrorMessage, internalError.Exception)
    {
        StatusCode = internalError.StatusCode;
    }
}