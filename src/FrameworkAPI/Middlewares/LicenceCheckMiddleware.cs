using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Http;

namespace FrameworkAPI.Middlewares;

public class LicenceCheckMiddleware(
    FieldDelegate next, ILicenceService licenceService, string? requiredLicence = null)
{
    // ReSharper disable once UnusedMember.Global
    public async Task InvokeAsync(IMiddlewareContext context)
    {
        var generateError = false;
        string? licence = null;
        string? machineId = null;

        if (requiredLicence is not null)
        {
            var hasValidLicence = await licenceService.HasValidLicence(requiredLicence!);

            if (!hasValidLicence)
            {
                generateError = true;
                licence = requiredLicence;
            }
        }

        if (!generateError)
        {
            // ToDo: Discuss/refine machine-specific license handling with machine-id context
            try
            {
                await next(context);
            }
            catch (InvalidLicenceException invalidLicenceException)
            {
                generateError = true;
                licence = invalidLicenceException.Licence;
                machineId = invalidLicenceException.MachineId;
            }
        }

        if (generateError)
        {
            var message =
                machineId is null
                    ? $"No valide '{licence}' licence found."
                    : $"No valide '{licence}' licence for machine {machineId} found.";

            var error = ErrorBuilder.New()
                .SetMessage(message)
                .SetCode(StatusCodes.Status402PaymentRequired.ToString())
                .SetPath(context.Path)
                .SetExtension("licence", licence)
                .SetExtension("machineId", machineId)
                .AddLocation(context.Selection.SyntaxNode)
                .Build();
            context.ReportError(error);
        }
    }
}