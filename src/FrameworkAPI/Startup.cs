using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WuH.Ruby.Common.ProjectTemplate;

namespace FrameworkAPI;

[ExcludeFromCodeCoverage]
internal class Startup
{
    private readonly string _applicationName;
    private static OpenTelemetryExceptionEventListener? OpenTelemetryExceptionEventListener;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        Environment = env;

        var assembly = typeof(Startup).GetTypeInfo().Assembly;
        _applicationName = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ??
                           throw new NullReferenceException("ProductName can't be null");
    }

    public IConfiguration Configuration { get; }

    public IWebHostEnvironment Environment { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        var openTelemetryEndpoint = new Uri(Configuration["OpenTelemetry:Endpoint"]!);
        var applicationVersion = typeof(Startup).Assembly.GetName().Version?.ToString() ?? "unknown";

        WuH.Ruby.Common.ProjectTemplate.ServiceCollectionExtensions.AddAuthentication(services);

        services
            .AddOptions(Configuration)
            .AddTracing(openTelemetryEndpoint, _applicationName, applicationVersion)
            .AddMetrics(openTelemetryEndpoint, _applicationName, applicationVersion)
            .AddHttpContextAccessor()
            .AddServicesFromCommonLib()
            .AddInternalServices()
            .AddSession()
            .AddDistributedMemoryCache()
            .AddGraphQlServices();

        if (Environment.IsDevelopment())
        {
            services.AddCorsPolicies(publicApi: false);
        }
        else
        {
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "GraphiQL/build";
            });
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseWebSockets();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSession();

        const string spaPath = "/wuh/graphiql";

        if (env.IsDevelopment())
        {
            app.UseCors("corsGlobalPolicy");

            // Comment this call if you want don't want to start the frontend
            app.MapWhen(httpContext => httpContext.Request.Path.StartsWithSegments(spaPath), client =>
            {
                client.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "GraphiQL";
                    spa.UseReactDevelopmentServer(npmScript: "start");
                });
            });
        }
        else
        {
            app.Map(new PathString(spaPath), client =>
            {
                client.UseSpaStaticFiles();
                client.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "GraphiQL";
                });
            });
        }

        app.UseEndpoints(endpoints =>
        {
            var graphQlOptions = new GraphQLServerOptions
            {
                Tool = { Enable = false }
            };

            endpoints.MapGraphQL().WithOptions(graphQlOptions);
            endpoints.MapGraphQLSchema("/graphql/schema");
        });
        OpenTelemetryExceptionEventListener = new OpenTelemetryExceptionEventListener();
        var linterSTFU = OpenTelemetryExceptionEventListener;
    }
}