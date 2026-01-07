using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;
using Common.Core.InternalResponses.Helper;
using FrameworkAPI.Filters;
using FrameworkAPI.Interceptors;
using FrameworkAPI.Mutations;
using FrameworkAPI.Queries;
using FrameworkAPI.Schema.Machine;
using FrameworkAPI.Schema.MachineTimeSpan;
using FrameworkAPI.Schema.MaterialLot;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.PhysicalAsset;
using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using FrameworkAPI.Schema.PhysicalAsset.Defect;
using FrameworkAPI.Schema.PhysicalAsset.History;
using FrameworkAPI.Schema.PhysicalAsset.Operation;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Schema.ProducedJob.MachineSettings.Extrusion;
using FrameworkAPI.Schema.Settings.DashboardSettings;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Services.Settings;
using FrameworkAPI.Subscriptions;
using HotChocolate.Diagnostics;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhysicalAssetDataHandler.Client.HttpClients;
using PhysicalAssetDataHandler.Client.QueueWrappers;
using WuH.Ruby.AlarmDataHandler.Client;
using WuH.Ruby.AlarmDataHandler.Client.QueueWrapper;
using WuH.Ruby.AlarmDataHandler.Client.QueueWrapper.Interface;
using WuH.Ruby.Common.Core;
using WuH.Ruby.Common.Http;
using WuH.Ruby.Common.ProjectTemplate;
using WuH.Ruby.Common.Queue;
using WuH.Ruby.Common.Track;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.LicenceManager.Client;
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Queue;
using WuH.Ruby.MaterialDataHandler.Client.HttpClient;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using WuH.Ruby.Settings.Client;
using WuH.Ruby.Supervisor.Client;

namespace FrameworkAPI;

[ExcludeFromCodeCoverage]
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures the settings by binding the contents of the config.json file to the specified Plain Old CLR
    /// Objects (POCO) and adding IOptions objects to the services collection.
    /// </summary>
    /// <param name="services">The services collection or IoC container.</param>
    /// <param name="configuration">Gets or sets the application configuration, where key value pair settings are
    /// stored.</param>
    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .Configure<DatabaseSettings>(configuration.GetSection(nameof(DatabaseSettings)))
            .Configure<ServiceSettings>(configuration.GetSection(nameof(ServiceSettings)))
            .Configure<QueueSettings>(configuration.GetSection(nameof(QueueSettings)));
    }

    /// <summary>
    /// Adds needed services from common lib.
    /// </summary>
    /// <param name="services">The services collection or IoC container.</param>
    public static IServiceCollection AddServicesFromCommonLib(this IServiceCollection services)
    {
        // Services that use http clients (typed clients)
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0
        services.AddCustomHttpClients<IMachineDataHandlerHttpClient, MachineDataHandlerHttpClient>();
        services.AddCustomHttpClients<IProductionPeriodsDataHandlerHttpClient, ProductionPeriodsDataHandlerHttpClient>();
        services.AddCustomHttpClients<IProcessDataReaderHttpClient, ProcessDataReaderHttpClient>();
        services.AddCustomHttpClients<IMaterialDataHandlerHttpClient, MaterialDataHandlerHttpClient>();
        services.AddCustomHttpClients<IMachineSnapshotHttpClient, MachineSnapshotHttpClient>();
        services.AddCustomHttpClients<IKpiDataHandlerClient, KpiDataHandlerClient>();
        services.AddCustomHttpClients<IMetaDataHandlerHttpClient, MetaDataHandlerHttpClient>();
        services.AddCustomHttpClients<ISettingsService, SettingsService>();
        services.AddCustomHttpClients<IShiftSettingsService, ShiftSettingsService>();
        services.AddCustomHttpClients<ITimeInformationSettingsService, TimeInformationSettingsService>();
        services.AddCustomHttpClients<IPhysicalAssetHttpClient, PhysicalAssetHttpClient>();
        services.AddCustomHttpClients<IPhysicalAssetSettingsHttpClient, PhysicalAssetSettingsHttpClient>();
        services.AddCustomHttpClients<ICapabilityTestSpecificationHttpClient, CapabilityTestSpecificationHttpClient>();
        services.AddCustomHttpClients<IAlarmDataHandlerHttpClient, AlarmDataHandlerHttpClient>();
        services.AddCustomHttpClients<ILicenceManagerHttpClient, LicenceManagerHttpClient>();
        services.AddCustomHttpClients<ISupervisorHttpClient, SupervisorHttpClient>();

        // Caching services
        services.AddSingleton<IProductionPeriodsCachingService, ProductionPeriodsCachingService>();
        services.AddSingleton<IMachineCachingService, MachineCachingService>();
        services.AddSingleton<IProcessDataCachingService, ProcessDataCachingService>();
        services.AddSingleton<ILatestMachineSnapshotCachingService, LatestMachineSnapshotCachingService>();
        services.AddSingleton<IMachineSnapshotSchemaCachingService, MachineSnapshotSchemaCachingService>();
        services.AddSingleton<IMetaDataCachingService, MetaDataCachingService>();
        services.AddSingleton<IKpiDataCachingService, KpiDataCachingService>();
        services.AddSingleton<IOpcUaServerTimeCachingService, OpcUaServerTimeCachingService>();
        services.AddSingleton<IMachineTrendCachingService, MachineTrendCachingService>();
        services.AddSingleton<IAlarmDataHandlerCachingService, AlarmDataHandlerCachingService>();
        services.AddSingleton<ILicenceManagerCachingService, LicenceManagerCachingService>();

        // Helper
        services.AddTransient<IHttpHandlingWrapper, HttpHandlingWrapper>();
        services.AddTransient(typeof(IInternalResponseHelper<>), typeof(InternalResponseHelper<>));
        services.AddSingleton<IJsonSchemaValidator, JsonSchemaValidator>();
        services.AddTransient<IQueueService, QueueService>();
        services.AddSingleton<IQueueConnectionProvider, QueueConnectionProvider>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddTransient<ISyncQueueService, SyncQueueService>();
        services.AddTransient<ISyncQueueConnectionProvider, SyncQueueConnectionProvider>();
#pragma warning restore CS0618 // Type or member is obsolete
        services.AddTransient<IQueueMessageHelper, QueueMessageHelper>();
        services.AddTransient<IMachineSnapshotQueueWrapper, MachineSnapshotQueueWrapper>();
        services.AddTransient<IProcessDataQueueWrapper, ProcessDataQueueWrapper>();
        services.AddSingleton<IKpiChangesQueueWrapper, KpiChangesQueueWrapper>();
        services.AddTransient<IKpiEventQueueWrapper, KpiEventQueueWrapper>();
        services.AddSingleton<IProductionPeriodChangesQueueWrapper, ProductionPeriodChangesQueueWrapper>();
        services.AddSingleton<IPhysicalAssetQueueWrapper, PhysicalAssetQueueWrapper>();
        services.AddSingleton<IAlarmQueueWrapper, AlarmQueueWrapper>();
        services.AddTransient<ITaskHelper, TaskHelper>();

        // ClaimsTransformation
        services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

        // Services
        services.AddScoped<IMachineService, MachineService>();
        services.AddScoped<IProducedJobService, ProducedJobService>();
        services.AddScoped<IProductGroupService, ProductGroupService>();
        services.AddSingleton<IMachineTimeService, MachineTimeService>();
        services.AddScoped<IMachineSnapshotService, MachineSnapshotService>();
        services.AddScoped<IMaterialConsumptionService, MaterialConsumptionService>();
        services.AddScoped<IKpiService, KpiService>();
        services.AddScoped<IColumnTrendService, ColumnTrendOfLast8HoursService>();
        services.AddScoped<IGlobalSettingsService, GlobalSettingsService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();
        services.AddScoped<IDashboardSettingsService, DashboardSettingsService>();
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<IExtrusionProfileService, ExtrusionProfileService>();
        services.AddScoped<IProcessDataService, ProcessDataService>();
        services.AddScoped<IAlarmService, AlarmService>();
        services.AddScoped<IMachineMetaDataService, MachineMetaDataService>();
        services.AddSingleton<ISnapshotColumnIdChangedTimestampCachingService, SnapshotColumnValueChangedTimestampCachingService>();
        services.AddSingleton<IJobInfoCachingService, JobInfoCachingService>();
        services.AddSingleton<IPhysicalAssetService, PhysicalAssetService>();
        services.AddSingleton<IPhysicalAssetOperationService, PhysicalAssetOperationService>();
        services.AddSingleton<IPhysicalAssetCapabilityTestSpecificationService, PhysicalAssetCapabilityTestSpecificationService>();
        services.AddSingleton<IPhysicalAssetCapabilityTestResultService, PhysicalAssetCapabilityTestResultService>();
        services.AddSingleton<ISchedulerProvider, SchedulerProvider>();
        services.AddSingleton<ILicenceService, LicenceService>();
        services.AddSingleton<ILicenceGuard, LicenceGuard>();
        services.AddSingleton<IMachineShiftService, MachineShiftService>();
        services.AddSingleton<ITrackProductionHistoryService, TrackProductionHistoryService>();

        // Common.Track Services
        services.AddSingleton<IHistoryEntryService, HistoryEntryService>();
        services.AddSingleton<ITrackSettingsService, TrackSettingsService>();

        // Etc.
        services.AddSingleton<IScheduler>(DefaultScheduler.Instance);

        return services;
    }

    /// <summary>
    /// Adds services implemented in this application.
    /// </summary>
    /// <param name="services">The services collection or IoC container.</param>
    public static IServiceCollection AddInternalServices(this IServiceCollection services)
    {
        services.AddSingleton<IStandardKpiChangesService, StandardKpiChangesService>();
        return services;
    }

    public static IRequestExecutorBuilder AddGraphQlServices(this IServiceCollection services)
    {
        return services
            .AddGraphQLServer()
            .ModifyOptions(schemaOptions => schemaOptions.SortFieldsByName = true)
            .ModifyRequestOptions(requestExecutorOptions => requestExecutorOptions.IncludeExceptionDetails = true)
            .AddMutationConventions()
            .AddAuthorization()
            .AddErrorFilter<CustomExceptionFilter>()
            .AddTypes()
            .AddQueries()
            .AddSubscriptions()
            .AddMutations()
            .AddFiltering()
            .AddSorting()
            .AddDefaultTransactionScopeHandler()
            .AddInMemorySubscriptions()
            .AddHttpRequestInterceptor<HttpRequestInterceptor>()
            .AddInstrumentation(option =>
            {
                option.RenameRootActivity = true;
                option.IncludeDocument = true;
                option.Scopes = ActivityScopes.All;
            })
            .AddApolloTracing(HotChocolate.Execution.Options.TracingPreference.OnDemand)
            .InitializeOnStartup();
    }

    private static IRequestExecutorBuilder AddTypes(this IRequestExecutorBuilder builder)
    {
        builder
            .AddType<PrintingMachine>()
            .AddType<PaperSackMachine>()
            .AddType<ExtrusionMachine>()
            .AddType<OtherMachine>()
            .AddType<PrintingMachineTimeSpan>()
            .AddType<PaperSackMachineTimeSpan>()
            .AddType<ExtrusionMachineTimeSpan>()
            .AddType<OtherMachineTimeSpan>()
            .AddType<ExtrusionProducedJob>()
            .AddType<PrintingProducedJob>()
            .AddType<PaperSackProducedJob>()
            .AddType<ExtrusionProducedRoll>()
            .AddType<ExtrusionMachineSettings>()
            .AddType<PhysicalAssetSettings>()
            .AddType<AniloxPhysicalAsset>()
            .AddType<PlatePhysicalAsset>()
            .AddType<Equipment>()
            .AddType<VolumeCapabilityTestSpecification>()
            .AddType<OpticalDensityCapabilityTestSpecification>()
            .AddType<AniloxCapabilityTestSpecification>()
            .AddType<VisualCapabilityTestSpecification>()
            .AddType<VolumeCapabilityTestResult>()
            .AddType<PhysicalAssetCreatedHistoryItem>()
            .AddType<PhysicalAssetDeliveredHistoryItem>()
            .AddType<PhysicalAssetHighVolumeHistoryItem>()
            .AddType<PhysicalAssetLowVolumeHistoryItem>()
            .AddType<PhysicalAssetVolumeMeasuredHistoryItem>()
            .AddType<PhysicalAssetCleanedHistoryItem>()
            .AddType<PhysicalAssetScrappedHistoryItem>()
            .AddType<PhysicalAssetScoringLineHistoryItem>()
            .AddType<PhysicalAssetSurfaceAnomalyHistoryItem>()
            .AddType<PhysicalAssetVolumeTriggeredPrintAnomalyHistoryItem>()
            .AddType<PhysicalAssetDefect>()
            .AddType<PhysicalAssetHighVolumeDefect>()
            .AddType<PhysicalAssetLowVolumeDefect>()
            .AddType<PhysicalAssetScoringLineDefect>()
            .AddType<PhysicalAssetSurfaceAnomalyDefect>()
            .AddType<PhysicalAssetVolumeTriggeredPrintAnomalyDefect>()
            .AddType<CleaningOperation>()
            .AddType<ScrappingOperation>()
            .AddType<TrackHistoryEntryType>()
            .AddType<TrackSetupHistoryEntry>()
            .AddType<TrackDowntimeHistoryEntry>()
            .AddType<TrackOfflineHistoryEntry>()
            .AddType<TrackProductionBreakHistoryEntry>()
            .AddType<TrackProductionHistoryEntry>()
            .AddType<TrackScrapHistoryEntry>()
            .AddType<DashboardSettings>()
            .AddType<DashboardWidgetSettings>()
            .AddType<CreateOrEditConfiguredDashboardRequest>();

        return builder;
    }

    private static IRequestExecutorBuilder AddQueries(this IRequestExecutorBuilder builder)
    {
        builder
            .AddQueryType(q => q.Name("Query").Authorize())
            .AddType<MachineQuery>()
            .AddType<MachineTimeSpanQuery>()
            .AddType<MaterialLotQuery>()
            .AddType<PhysicalAssetQuery>()
            .AddType<ProducedJobQuery>()
            .AddType<SettingsQuery>()
            .AddType<PaperSackProductGroupQuery>();
        return builder;
    }

    private static IRequestExecutorBuilder AddSubscriptions(this IRequestExecutorBuilder builder)
    {
        builder
            .AddSubscriptionType(q => q.Name("Subscription").Authorize())
            .AddType<MachineChangedSubscription>()
            .AddType<MachineTimeChangedSubscription>()
            .AddType<PhysicalAssetChangedSubscription>()
            .AddType<PhysicalAssetScrappedSubscription>();
        return builder;
    }

    private static IRequestExecutorBuilder AddMutations(this IRequestExecutorBuilder builder)
    {
        builder
            .AddMutationType(q => q.Name("Mutation").Authorize())
            .AddType<UserSettingsMutation>()
            .AddType<GlobalSettingsMutation>()
            .AddType<DashboardSettingsMutation>()
            .AddType<PhysicalAssetsMutation>()
            .AddType<ProductGroupsMutation>()
            .AddType<ProducedJobsMutation>();
        return builder;
    }
}