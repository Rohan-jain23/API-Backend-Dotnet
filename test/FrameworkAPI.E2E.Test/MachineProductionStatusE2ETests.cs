using System;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.E2E.Client;
using FrameworkAPI.E2E.Test.Helper;
using Microsoft.Extensions.DependencyInjection;
using WuH.Ruby.Common.Testing;
using Xunit;
using Xunit.Abstractions;

namespace FrameworkAPI.E2E.Test;

// Not allowed to run in parallel with MachineOverviewE2ETests 
[Collection("Fast")]
[TestCaseOrderer(
    ordererTypeName: "FrameworkAPI.E2E.Test.Helper.OrderTestCasesByAlphabet",
    ordererAssemblyName: "FrameworkAPI.E2E.Test")]
public class MachineProductionStatusE2ETests
{
    // Simulation
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IFrameworkAPIClient _frameworkApiClient;
    private const string MachineSnapShooterContainerName = "MachineSnapShooter";
    private const string MachineSimulationContainerName = "Machine Simulation";
    private const string HostnameUri = "lx64ispft4.wuh-intern.de";

    public MachineProductionStatusE2ETests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var baseUri = E2EHelper.InitializeAndCreateUriBasedOnOperatingSystem(HostnameUri);

        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddFrameworkAPIClient()
            .ConfigureHttpClient(
                httpClient =>
                {
                    var accessToken = E2EClientInitializer.GetInternalAccessToken(testOutputHelper);

                    httpClient.BaseAddress = new Uri($"{baseUri}/graphql");
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);
                },
                httpClientBuilder =>
                {
                    httpClientBuilder.ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    });
                });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        _frameworkApiClient = serviceProvider.GetRequiredService<IFrameworkAPIClient>();
    }

    [Fact]
    public async Task MachineStatus_Stays_ScheduledNonProduction_When_Machine_Connects_After_Disconnect_And_MachineSnapShooter_Restarts()
    {
        // Arrange
        const string expectedMachineId = "EQ10211";

        // Act
        // Prepare that everything is up and running and start scheduled non production
        await AdminApiHelper.EnsureApplicationStarted(_testOutputHelper, MachineSimulationContainerName);
        MachineSimulationHelper.EnsureMachineSimulationIsReady(_testOutputHelper, expectedMachineId);
        await Task.Delay(TimeSpan.FromSeconds(10));
        MachineSimulationHelper.Start(_testOutputHelper, expectedMachineId);
        await Task.Delay(TimeSpan.FromSeconds(20));
        var machinePreparationData = await GetPaperSackTuberMachine();
        if (machinePreparationData.ProductionStatus.Category == ProductionStatusCategory.ScheduledNonProduction)
        {
            _testOutputHelper.WriteLineWithTimestamp($"{expectedMachineId} is already in scheduled non production");
            OperatorUiApiHelper.EndScheduledNonProduction(_testOutputHelper, expectedMachineId);
            _testOutputHelper.WriteLineWithTimestamp("Ended scheduled non production. Waiting for Snapshot.");
            await Task.Delay(TimeSpan.FromMinutes(2));
        }
        OperatorUiApiHelper.StartScheduledNonProduction(_testOutputHelper, expectedMachineId);

        // Check if production status is scheduled non production
        await Task.Delay(TimeSpan.FromMinutes(1));
        var machineDataAfterStartOfScheduledNonProduction = await GetPaperSackTuberMachine();
        machineDataAfterStartOfScheduledNonProduction.ProductionStatus.Category.Should().Be(ProductionStatusCategory.ScheduledNonProduction);

        // Stop machine simulation and restart MachineSnapShooter and check if production status is offline
        await AdminApiHelper.EnsureApplicationStopped(_testOutputHelper, MachineSimulationContainerName);
        await Task.Delay(TimeSpan.FromMinutes(2));
        await AdminApiHelper.EnsureApplicationRestarted(_testOutputHelper, MachineSnapShooterContainerName);
        await Task.Delay(TimeSpan.FromMinutes(3));
        var machineDataBeforeMachineStart = await GetPaperSackTuberMachine();
        machineDataBeforeMachineStart.ProductionStatus.Category.Should().Be(ProductionStatusCategory.Offline);
        var timestampForLaterCheck = machineDataBeforeMachineStart.Time;

        // Start simulation and machine and check status
        await AdminApiHelper.EnsureApplicationStarted(_testOutputHelper, MachineSimulationContainerName);
        MachineSimulationHelper.EnsureMachineSimulationIsReady(_testOutputHelper, expectedMachineId);
        await Task.Delay(TimeSpan.FromSeconds(10));
        MachineSimulationHelper.Start(_testOutputHelper, expectedMachineId);
        await Task.Delay(TimeSpan.FromSeconds(20));
        var machineDataAfterMachineStartButStillScheduledNonProduction = await GetPaperSackTuberMachine();
        machineDataAfterMachineStartButStillScheduledNonProduction.ProductionStatus.Category.Should().Be(ProductionStatusCategory.ScheduledNonProduction);

        // Stop Scheduled Non Production and check the status in the FrameworkAPI
        OperatorUiApiHelper.EndScheduledNonProduction(_testOutputHelper, expectedMachineId);
        await Task.Delay(TimeSpan.FromSeconds(20));
        var machineDataAfterMachineStartButWithoutScheduledNonProduction = await GetPaperSackTuberMachine();
        machineDataAfterMachineStartButWithoutScheduledNonProduction.ProductionStatus.Category.Should().NotBe(ProductionStatusCategory.ScheduledNonProduction);

        // Check if MachineSnapShooter changed old offline status to scheduled non production 
        var productionStatusByTimestamp = await _frameworkApiClient.GetProductionStatusByTimestamp.ExecuteAsync(timestampForLaterCheck, expectedMachineId);
        productionStatusByTimestamp.Data!.Machine.ProductionStatus.Category.Should().Be(ProductionStatusCategory.ScheduledNonProduction);
    }

    private async Task<IGetPaperSackTuberMachine_Machine> GetPaperSackTuberMachine()
    {
        var machine = await _frameworkApiClient.GetPaperSackTuberMachine.ExecuteAsync();
        if (machine.Data is null)
        {
            throw new NoNullAllowedException("Machine data is null");
        }
        return machine.Data.Machine;
    }
}