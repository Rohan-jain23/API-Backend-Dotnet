using System;

namespace FrameworkAPI.Exceptions;

public class ParameterInvalidException(string message) : Exception(message)
{
}