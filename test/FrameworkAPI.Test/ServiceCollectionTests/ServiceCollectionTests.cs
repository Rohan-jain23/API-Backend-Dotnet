using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace FrameworkAPI.Test.ServiceCollectionTests;

public class ServiceCollectionTests
{
    [Fact]
    public void AllServicesShouldConstructSuccessfully()
    {
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
                webBuilder
                    .UseDefaultServiceProvider((_, options) =>
                    {
                        options.ValidateScopes = true;
                        options.ValidateOnBuild = true;
                    })
                    .UseStartup<Startup>())
            .Build();
    }
}