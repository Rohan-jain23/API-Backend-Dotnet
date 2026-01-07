using System;

namespace FrameworkAPI.Exceptions;

public class IdNotFoundException : Exception
{
    public IdNotFoundException() : base("An object with the requested id does not exist.")
    {
    }

    public IdNotFoundException(string id) : base($"An object with the requested id '{id}' does not exist.")
    {
    }
}