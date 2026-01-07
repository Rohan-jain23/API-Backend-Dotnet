using System;
using System.Reflection;
using FrameworkAPI.Middlewares;
using FrameworkAPI.Services.Interfaces;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using Microsoft.Extensions.DependencyInjection;

namespace FrameworkAPI.Attributes;

[AttributeUsage(
    AttributeTargets.Struct |
    AttributeTargets.Property |
    AttributeTargets.Method,
    AllowMultiple = true)]
public class LicenceCheck : DescriptorAttribute
{
    public string? Licence { get; set; }

    protected override void TryConfigure(IDescriptorContext context, IDescriptor descriptor, ICustomAttributeProvider element)
    {
        if (descriptor is IObjectFieldDescriptor field)
        {
            field.Use<LicenceCheckMiddleware>((services, next) =>
            {
                if (Licence is null)
                {
                    return new LicenceCheckMiddleware(next, services.GetRequiredService<ILicenceService>());
                }

                ArgumentException.ThrowIfNullOrWhiteSpace(Licence);
                return new LicenceCheckMiddleware(
                    next, services.GetRequiredService<ILicenceService>(), Licence);
            });
        }
    }
}