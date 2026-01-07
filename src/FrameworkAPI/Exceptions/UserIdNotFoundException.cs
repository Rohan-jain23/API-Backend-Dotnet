using System;

namespace FrameworkAPI.Exceptions;

public class UserIdNotFoundException() : Exception("UserId is not set in request header.")
{
}