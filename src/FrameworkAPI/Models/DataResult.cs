using System;

namespace FrameworkAPI.Models;

public class DataResult<T>
{
    public DataResult(T? value, Exception? exception)
    {
        if (value is not null && exception is not null)
        {
            throw new ArgumentException($"{nameof(value)} and {nameof(exception)} are not null.");
        }

        Value = value;
        Exception = exception;
    }

    public T? Value { get; }

    public Exception? Exception { get; }

    public void Deconstruct(out T? value, out Exception? exception)
    {
        value = Value;
        exception = Exception;
    }

    public T? GetValueOrThrow()
    {
        if (Exception is not null)
        {
            throw Exception;
        }

        return Value;
    }
}