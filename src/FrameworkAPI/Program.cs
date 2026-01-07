using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog;
using WuH.Ruby.Common.ProjectTemplate;

namespace FrameworkAPI;

[ExcludeFromCodeCoverage]
internal class Program
{
    public static void Main(string[] args)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        ThreadPool.SetMinThreads(50, 100);

        BuildWebHost(args).Run();
    }

    public static IHost BuildWebHost(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder
                        .ConfigureWuhLogging()
                        .UseStartup<Startup>())
                .Build();
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = (Exception)e.ExceptionObject;
        var logger = LogManager.GetCurrentClassLogger();
        logger.Fatal(exception, "Caught an unhandled exception in '{Source}'. Message: {Message}", exception.Source, exception.Message);
        LogManager.Shutdown();
    }
}