using System;

namespace FrameworkAPI.E2E.Test.Helper;

public abstract class E2EHelper
{
    public static string InitializeAndCreateUriBasedOnOperatingSystem(string hostnameUri)
    {
        var isLocalDev = false;
        if (OperatingSystem.IsMacOS() || OperatingSystem.IsWindows())
        {
            isLocalDev = true;
            Environment.SetEnvironmentVariable("ISP_HOSTNAME", hostnameUri);
        }

        return $"https://{(isLocalDev ? hostnameUri : Environment.GetEnvironmentVariable("ISP_HOSTNAME"))}";
    }
}