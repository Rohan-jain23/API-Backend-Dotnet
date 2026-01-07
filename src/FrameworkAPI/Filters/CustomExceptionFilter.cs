using FrameworkAPI.Exceptions;
using HotChocolate;

namespace FrameworkAPI.Filters;

public class CustomExceptionFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is ColumnDoesNotExistForMachineException exception)
        {
            // if we do not build a new error, the message is also part of extensions and we also have the stack trace
            var errorBuilder = ErrorBuilder.New()
                .SetMessage(exception.Message)
                .SetPath(error.Path)
                .SetExtension("errorCode", "COLUMN_DOES_NOT_EXIST_FOR_MACHINE");

            // There is no setter to set all message at a time
            if (error.Locations != null)
            {
                foreach (var location in error.Locations)
                {
                    errorBuilder.AddLocation(location.Line, location.Column);
                }
            }
            return errorBuilder.Build();
        }

        // Let other errors pass through unmodified
        return error;
    }
}