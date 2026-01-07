using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using FrameworkAPI.E2E.Client;
using FrameworkAPI.E2E.Test.Helper;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;
using WuH.Ruby.Common.Testing;
using Xunit;
using Xunit.Abstractions;

namespace FrameworkAPI.E2E.Test;

// Not allowed to run in parallel with MachineProductionStatusE2ETests 
[Collection("Slow")]
[TestCaseOrderer(
    ordererTypeName: "FrameworkAPI.E2E.Test.Helper.OrderTestCasesByAlphabet",
    ordererAssemblyName: "FrameworkAPI.E2E.Test")]
public class MachineOverviewE2ETests
{
    // Simulation
    private static readonly TimeSpan SimulateProductionPhaseTimeSpan = TimeSpan.FromMinutes(2);
    private const string OpcUaForwarderContainerName = "OPC UA Forwarder";

    private const string ExpectedPrefix = "MachineOverviewE2ETests";
    private const string ExpectedMachineName = "GBD Machine Simulation";
    private const string ExpectedCustomer = $"{ExpectedPrefix}Customer";
    private static readonly string ExpectedJob = $"{ExpectedPrefix}Job{DateTime.UtcNow:O}";
    private const double ExpectedJobSize = 1234.0;
    private const string ExpectedProductId = $"{ExpectedPrefix}ProductId";
    private const double ExpectedSpeed = 567.0;

    // Printing only
    private static readonly double ExpectedPrintingGoodLength =
        ExpectedSpeed * SimulateProductionPhaseTimeSpan.TotalMinutes;

    // PaperSack only
    private static readonly double ExpectedGoodQuantity = ExpectedSpeed * SimulateProductionPhaseTimeSpan.TotalMinutes;
    private const string ExpectedMaterialInformation = $"{ExpectedPrefix}MaterialInformation";
    private const string ExpectedMaterialText = $"{ExpectedPrefix}MaterialText";

    // Extrusion only
    private static readonly double ExpectedExtrusionGoodLength =
        ExpectedLineSpeed * SimulateProductionPhaseTimeSpan.TotalMinutes;

    // kg/h / 60 = kg/min
    private static readonly double ExpectedGoodWeight =
        ExpectedSpeed / 60.0 * SimulateProductionPhaseTimeSpan.TotalMinutes;

    private const double ExpectedThickness = 123.0;
    private const double ExpectedWidth = 456.0;
    private const double ExpectedTwoSigma = 789.0;
    private const double ExpectedLineSpeed = ExpectedSpeed * 0.06;
    private const double ExpectedThroughputRate = ExpectedSpeed;
    private const int ExpectedProfileMean = 100;
    private const int ExpectedDataPointsCount = 480;
    private const float ExpectedDataPointsValue = 100f;

    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IFrameworkAPIClient _frameworkApiClient;
    private const string HostnameUri = "lx64ispft4.wuh-intern.de";

    public MachineOverviewE2ETests(ITestOutputHelper testOutputHelper)
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
    public async Task Should_Get_Queried_FlexoPrint_Data()
    {
        const string expectedMachineId = "EQ10101";

        await PerformTest(
            expectedMachineId,
            setupAction: () =>
            {
                // Arrange
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Printing.FlexoPrint.Customer,
                    ExpectedCustomer);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Printing.FlexoPrint.JobSize, ExpectedJobSize);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Printing.FlexoPrint.Speed, ExpectedSpeed);
            },
            verifyQueryResponseWhileMachineIsInProductionFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetFlexoPrintMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;
                machine.Department.Should().Be(MachineDepartment.Printing);
                machine.MachineFamily.Should().Be(MachineFamily.FlexoPrint);
                machine.MachineType.Should().Be("MIRAFLEX\"");
                machine.MachineId.Should().Be(expectedMachineId);
                machine.Name.Should().Be(ExpectedMachineName);

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                producedJob.Customer.LastValue.Should().Be(ExpectedCustomer);
                producedJob.EndTime.Should().BeNull();
                producedJob.IsActive.Should().BeTrue();
                producedJob.JobId.Should().StartWith(ExpectedJob);
                producedJob.JobSize.LastValue.Should().Be(ExpectedJobSize);
                producedJob.JobSize.Unit.Should().Be("m");
                producedJob.MachineId.Should().Be(machine.MachineId);
                producedJob.ProductId.Should().Be(ExpectedProductId);
                // start time
                producedJob.UniqueId.Should().Be($"{producedJob.MachineId}_{producedJob.JobId}");

                machine.ProductionStatus.Category.Should().Be(ProductionStatusCategory.Production);
                machine.ProductionStatus.Id.Should().Be(4);
                // start time
                // time

                var printingMachine = machine as GetFlexoPrintMachine_Machine_PrintingMachine;
                printingMachine.Should().NotBeNull();
                printingMachine!.Speed.Unit.Should().Be("m/min");
                printingMachine.Speed.Value.Should().Be(ExpectedSpeed);
            },
            verifyQueryResponseAfterMachineSimulationIsStoppedFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetFlexoPrintMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                var printingProducedJob =
                    producedJob as GetFlexoPrintMachine_Machine_ProducedJob_PrintingProducedJob;
                printingProducedJob.Should().NotBeNull();
                printingProducedJob!.GoodLength.Unit.Should().Be("m");
                printingProducedJob.GoodLength.Value.Should()
                    .BeApproximately(ExpectedPrintingGoodLength, precision: 110.0);
                printingProducedJob.ScrapLength.Unit.Should().Be("m");
                printingProducedJob.ScrapLength.Value.Should().Be(0.0);
            });
    }

    [Fact]
    public async Task Should_Get_Queried_OldFlexoPrint_Data()
    {
        const string expectedMachineId = "EQ10102";

        await PerformTest(
            expectedMachineId,
            setupAction: () =>
            {
                // Arrange
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Printing.OldFlexoPrint.Customer,
                    ExpectedCustomer);
                // There are two process variables for the job size. We set both because it is not deterministic
                // which one will be actually in charge
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Printing.OldFlexoPrint.JobSize,
                    ExpectedJobSize);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Printing.OldFlexoPrint.JobSizeAlternative,
                    ExpectedJobSize);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Printing.OldFlexoPrint.Speed, ExpectedSpeed);
            },
            verifyQueryResponseWhileMachineIsInProductionFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetOldFlexoPrintMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;
                machine.Department.Should().Be(MachineDepartment.Printing);
                machine.MachineFamily.Should().Be(MachineFamily.FlexoPrint);
                machine.MachineType.Should().Be("VISTAFLEX");
                machine.MachineId.Should().Be(expectedMachineId);
                machine.Name.Should().Be(ExpectedMachineName);

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                producedJob.Customer.LastValue.Should().Be(ExpectedCustomer);
                producedJob.EndTime.Should().BeNull();
                producedJob.IsActive.Should().BeTrue();
                producedJob.JobId.Should().StartWith(ExpectedJob);
                producedJob.JobSize.LastValue.Should().Be(ExpectedJobSize);
                producedJob.JobSize.Unit.Should().Be("m");
                producedJob.MachineId.Should().Be(machine.MachineId);
                producedJob.ProductId.Should().Be(ExpectedProductId);
                // start time
                producedJob.UniqueId.Should().Be($"{producedJob.MachineId}_{producedJob.JobId}");

                machine.ProductionStatus.Category.Should().Be(ProductionStatusCategory.Production);
                machine.ProductionStatus.Id.Should().Be(151);
                // start time
                // time

                var printingMachine = machine as GetOldFlexoPrintMachine_Machine_PrintingMachine;
                printingMachine.Should().NotBeNull();
                printingMachine!.Speed.Unit.Should().Be("m/min");
                printingMachine.Speed.Value.Should().Be(ExpectedSpeed);
            },
            verifyQueryResponseAfterMachineSimulationIsStoppedFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetOldFlexoPrintMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                var printingProducedJob =
                    producedJob as GetOldFlexoPrintMachine_Machine_ProducedJob_PrintingProducedJob;
                printingProducedJob.Should().NotBeNull();
                printingProducedJob!.GoodLength.Unit.Should().Be("m");
                printingProducedJob.GoodLength.Value.Should()
                    .BeApproximately(ExpectedPrintingGoodLength, precision: 110.0);
                printingProducedJob.ScrapLength.Unit.Should().Be("m");
                printingProducedJob.ScrapLength.Value.Should().Be(0.0);
            });
    }

    [Fact]
    public async Task Should_Get_Queried_GravurePrint_Data()
    {
        const string expectedMachineId = "EQ10111";

        await PerformTest(
            expectedMachineId,
            setupAction: () =>
            {
                // Arrange
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Printing.GravurePrint.Customer,
                    ExpectedCustomer);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Printing.GravurePrint.JobSize,
                    ExpectedJobSize);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Printing.GravurePrint.Speed, ExpectedSpeed);
            },
            verifyQueryResponseWhileMachineIsInProductionFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetGravurePrintMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;
                machine.Department.Should().Be(MachineDepartment.Printing);
                machine.MachineFamily.Should().Be(MachineFamily.GravurePrint);
                machine.MachineType.Should().Be("HELIOSTAR\"");
                machine.MachineId.Should().Be(expectedMachineId);
                machine.Name.Should().Be(ExpectedMachineName);

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                producedJob.Customer.LastValue.Should().Be(ExpectedCustomer);
                producedJob.EndTime.Should().BeNull();
                producedJob.IsActive.Should().BeTrue();
                producedJob.JobId.Should().StartWith(ExpectedJob);
                producedJob.JobSize.LastValue.Should().Be(ExpectedJobSize);
                producedJob.JobSize.Unit.Should().Be("m");
                producedJob.MachineId.Should().Be(machine.MachineId);
                producedJob.ProductId.Should().Be(ExpectedProductId);
                // start time
                producedJob.UniqueId.Should().Be($"{producedJob.MachineId}_{producedJob.JobId}");

                machine.ProductionStatus.Category.Should().Be(ProductionStatusCategory.Production);
                machine.ProductionStatus.Id.Should().Be(4);
                // start time
                // time

                var printingMachine = machine as GetGravurePrintMachine_Machine_PrintingMachine;
                printingMachine.Should().NotBeNull();
                printingMachine!.Speed.Unit.Should().Be("m/min");
                printingMachine.Speed.Value.Should().Be(ExpectedSpeed);
            },
            verifyQueryResponseAfterMachineSimulationIsStoppedFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetGravurePrintMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                var printingProducedJob = producedJob as GetGravurePrintMachine_Machine_ProducedJob_PrintingProducedJob;
                printingProducedJob.Should().NotBeNull();
                printingProducedJob!.GoodLength.Unit.Should().Be("m");
                printingProducedJob.GoodLength.Value.Should()
                    .BeApproximately(ExpectedPrintingGoodLength, precision: 110.0);
                printingProducedJob.ScrapLength.Unit.Should().Be("m");
                printingProducedJob.ScrapLength.Value.Should().Be(0.0);
            });
    }

    [Fact]
    public async Task Should_Get_Queried_PaperSackTuber_Data()
    {
        const string expectedMachineId = "EQ10211";

        const double expectedTubeLengthInMillimeter = 123.0;
        const double expectedTubeWidthInMillimeter = 456.0;

        await PerformTest(
            expectedMachineId,
            setupAction: () =>
            {
                // Arrange
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.PaperSack.Customer, ExpectedCustomer);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.PaperSack.JobSize, ExpectedJobSize);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.PaperSack.Tuber.TubeLength,
                    expectedTubeLengthInMillimeter);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.PaperSack.Tuber.TubeWidth, expectedTubeWidthInMillimeter);
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.PaperSack.MaterialInformation,
                    ExpectedMaterialInformation);
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.PaperSack.MaterialText, ExpectedMaterialText);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.PaperSack.Speed, ExpectedSpeed);
            },
            verifyQueryResponseWhileMachineIsInProductionFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetPaperSackTuberMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;
                machine.Department.Should().Be(MachineDepartment.PaperSack);
                machine.MachineFamily.Should().Be(MachineFamily.PaperSackTuber);
                machine.MachineType.Should().Be("AM8735");
                machine.MachineId.Should().Be(expectedMachineId);
                machine.Name.Should().Be(ExpectedMachineName);

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                producedJob.Customer.LastValue.Should().Be(ExpectedCustomer);
                producedJob.EndTime.Should().BeNull();
                producedJob.IsActive.Should().BeTrue();
                producedJob.JobId.Should().StartWith(ExpectedJob);
                producedJob.JobSize.LastValue.Should().Be(ExpectedJobSize);
                producedJob.JobSize.Unit.Should().Be("unit.items");
                producedJob.MachineId.Should().Be(machine.MachineId);
                producedJob.ProductId.Should().Be(ExpectedProductId);
                // start time
                producedJob.UniqueId.Should().Be($"{producedJob.MachineId}_{producedJob.JobId}");

                var paperSackProducedJob =
                    producedJob as GetPaperSackTuberMachine_Machine_ProducedJob_PaperSackProducedJob;
                paperSackProducedJob.Should().NotBeNull();

                var machineSettings = paperSackProducedJob!.MachineSettings;
                machineSettings.CutType.Should().NotBeNull();
                machineSettings.CutType!.LastValue.Should().Be(PaperSackCutType.SteppedEnd);
                machineSettings.TubeLength.Should().NotBeNull();
                machineSettings.TubeLength!.LastValue.Should().BeApproximately(expectedTubeLengthInMillimeter, 0.01);
                machineSettings.TubeLength.Unit.Should().Be("mm");
                machineSettings.TubeWidth.Should().NotBeNull();
                machineSettings.TubeWidth!.LastValue.Should().Be(expectedTubeWidthInMillimeter);
                machineSettings.TubeWidth.Unit.Should().Be("mm");

                paperSackProducedJob.MaterialInformation.LastValue.Should().Be(ExpectedMaterialInformation);
                paperSackProducedJob.MaterialText.LastValue.Should().Be(ExpectedMaterialText);

                machine.ProductionStatus.Category.Should().Be(ProductionStatusCategory.Production);
                machine.ProductionStatus.Id.Should().Be(201);
                // start time
                // time

                var paperSackMachine = machine as GetPaperSackTuberMachine_Machine_PaperSackMachine;
                paperSackMachine.Should().NotBeNull();
                paperSackMachine!.Speed.Unit.Should().Be("unit.itemsPerMinute");
                paperSackMachine.Speed.Value.Should().Be(ExpectedSpeed);
            },
            verifyQueryResponseAfterMachineSimulationIsStoppedFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetPaperSackTuberMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                var paperSackProducedJob =
                    producedJob as GetPaperSackTuberMachine_Machine_ProducedJob_PaperSackProducedJob;
                paperSackProducedJob.Should().NotBeNull();
                paperSackProducedJob!.GoodQuantity.Unit.Should().Be("unit.items");
                paperSackProducedJob.GoodQuantity.Value.Should()
                    .BeApproximately(ExpectedGoodQuantity, precision: 110.0);
                paperSackProducedJob.ScrapQuantity.Unit.Should().Be("unit.items");
                paperSackProducedJob.ScrapQuantity.Value.Should()
                    .BeApproximately(0.0, precision: ExpectedSpeed / 12); // There might be some setup scrap because of sampling problems
            });
    }

    [Fact]
    public async Task Should_Get_Queried_PaperSackBottomer_Data()
    {
        const string expectedMachineId = "EQ10221";
        const double expectedSackLength = 123.0;
        const double expectedSackWidth = 456.0;
        const double expectedStandUpBottomWidth = 789.0;
        const double expectedValveBottomWidth = 12.0;
        const string expectedValveUnit1IsActive = "True";

        await PerformTest(
            expectedMachineId,
            setupAction: () =>
            {
                // Arrange
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.PaperSack.Customer, ExpectedCustomer);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.PaperSack.JobSize, ExpectedJobSize);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.PaperSack.Bottomer.SackLength,
                    expectedSackLength);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.PaperSack.Bottomer.SackWidth,
                    expectedSackWidth);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.PaperSack.Bottomer.StandUpBottomWidth,
                    expectedStandUpBottomWidth);
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.PaperSack.Bottomer.ValveUnit1IsActive,
                    expectedValveUnit1IsActive);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.PaperSack.Bottomer.ValveBottomWidth,
                    expectedValveBottomWidth);
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.PaperSack.MaterialInformation,
                    ExpectedMaterialInformation);
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.PaperSack.MaterialText, ExpectedMaterialText);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.PaperSack.Speed, ExpectedSpeed);
            },
            verifyQueryResponseWhileMachineIsInProductionFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetPaperSackBottomerMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;
                machine.Department.Should().Be(MachineDepartment.PaperSack);
                machine.MachineFamily.Should().Be(MachineFamily.PaperSackBottomer);
                machine.MachineType.Should().Be("AD8930");
                machine.MachineId.Should().Be(expectedMachineId);
                machine.Name.Should().Be(ExpectedMachineName);

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                producedJob.Customer.LastValue.Should().Be(ExpectedCustomer);
                producedJob.EndTime.Should().BeNull();
                producedJob.IsActive.Should().BeTrue();
                producedJob.JobId.Should().StartWith(ExpectedJob);
                producedJob.JobSize.LastValue.Should().Be(ExpectedJobSize);
                producedJob.JobSize.Unit.Should().Be("unit.items");
                producedJob.MachineId.Should().Be(machine.MachineId);
                producedJob.ProductId.Should().Be(ExpectedProductId);
                // start time
                producedJob.UniqueId.Should().Be($"{producedJob.MachineId}_{producedJob.JobId}");

                var paperSackProducedJob =
                    producedJob as GetPaperSackBottomerMachine_Machine_ProducedJob_PaperSackProducedJob;
                paperSackProducedJob.Should().NotBeNull();

                var machineSettings = paperSackProducedJob!.MachineSettings;
                machineSettings.SackLength.Should().NotBeNull();
                machineSettings.SackLength!.LastValue.Should().Be(expectedSackLength);
                machineSettings.SackLength.Unit.Should().Be("mm");
                machineSettings.SackWidth.Should().NotBeNull();
                machineSettings.SackWidth!.LastValue.Should().Be(expectedSackWidth);
                machineSettings.SackWidth.Unit.Should().Be("mm");
                machineSettings.StandUpBottomWidth.Should().NotBeNull();
                machineSettings.StandUpBottomWidth!.LastValue.Should().Be(expectedStandUpBottomWidth);
                machineSettings.StandUpBottomWidth.Unit.Should().Be("mm");
                machineSettings.ValveBottomWidth.Should().NotBeNull();
                machineSettings.ValveBottomWidth!.LastValue.Should().Be(expectedValveBottomWidth);
                machineSettings.ValveBottomWidth.Unit.Should().Be("mm");

                paperSackProducedJob.MaterialInformation.LastValue.Should().Be(ExpectedMaterialInformation);
                paperSackProducedJob.MaterialText.LastValue.Should().Be(ExpectedMaterialText);

                machine.ProductionStatus.Category.Should().Be(ProductionStatusCategory.Production);
                machine.ProductionStatus.Id.Should().Be(201);
                // start time
                // time

                var paperSackMachine = machine as GetPaperSackBottomerMachine_Machine_PaperSackMachine;
                paperSackMachine.Should().NotBeNull();
                paperSackMachine!.Speed.Unit.Should().Be("unit.itemsPerMinute");
                paperSackMachine.Speed.Value.Should().Be(ExpectedSpeed);
            },
            verifyQueryResponseAfterMachineSimulationIsStoppedFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetPaperSackBottomerMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                var paperSackProducedJob =
                    producedJob as GetPaperSackBottomerMachine_Machine_ProducedJob_PaperSackProducedJob;
                paperSackProducedJob.Should().NotBeNull();
                paperSackProducedJob!.GoodQuantity.Unit.Should().Be("unit.items");
                paperSackProducedJob.GoodQuantity.Value.Should()
                    .BeApproximately(ExpectedGoodQuantity, precision: 110.0);
                paperSackProducedJob.ScrapQuantity.Unit.Should().Be("unit.items");
                paperSackProducedJob.ScrapQuantity.Value.Should()
                    .BeApproximately(0.0, precision: ExpectedSpeed / 12); // There might be some setup scrap because of sampling problems
            });
    }

    [Fact]
    public async Task Should_Get_Queried_BlowFilm_Data()
    {
        const string expectedMachineId = "EQ10301";

        await PerformTest(
            expectedMachineId,
            setupAction: () =>
            {
                // Arrange
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Extrusion.Customer, ExpectedCustomer);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Extrusion.JobSize, ExpectedJobSize);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Extrusion.Thickness, ExpectedThickness);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Extrusion.BlowFilm.Width, ExpectedWidth);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.BlowFilm.TwoSigma,
                    ExpectedTwoSigma);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.BlowFilm.IsThicknessGaugeOn,
                    1);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.BlowFilm.LineSpeed,
                    ExpectedLineSpeed);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.ThroughputRate,
                    ExpectedThroughputRate);
            },
            verifyQueryResponseWhileMachineIsInProductionFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetBlowFilmMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;
                machine.Department.Should().Be(MachineDepartment.Extrusion);
                machine.MachineFamily.Should().Be(MachineFamily.BlowFilm);
                machine.MachineType.Should().Be("VAREX");
                machine.MachineId.Should().Be(expectedMachineId);
                machine.Name.Should().Be(ExpectedMachineName);

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                producedJob.Customer.LastValue.Should().Be(ExpectedCustomer);
                producedJob.EndTime.Should().BeNull();
                producedJob.IsActive.Should().BeTrue();
                producedJob.JobId.Should().StartWith(ExpectedJob);
                producedJob.JobSize.LastValue.Should().Be(ExpectedJobSize);
                producedJob.JobSize.Unit.Should().Be("m");
                producedJob.MachineId.Should().Be(machine.MachineId);
                producedJob.ProductId.Should().Be(ExpectedProductId);
                // start time
                producedJob.UniqueId.Should().Be($"{producedJob.MachineId}_{producedJob.JobId}");

                var extrusionProducedJob = producedJob as GetBlowFilmMachine_Machine_ProducedJob_ExtrusionProducedJob;
                extrusionProducedJob.Should().NotBeNull();

                var machineSettings = extrusionProducedJob!.MachineSettings;
                machineSettings.Thickness.Should().NotBeNull();
                machineSettings.Thickness.LastValue.Should().Be(ExpectedThickness);
                machineSettings.Thickness.Unit.Should().Be("µm");
                machineSettings.Width.Should().NotBeNull();
                machineSettings.Width.LastValue.Should().Be(ExpectedWidth);
                machineSettings.Width.Unit.Should().Be("mm");

                machine.ProductionStatus.Category.Should().Be(ProductionStatusCategory.Production);
                machine.ProductionStatus.Id.Should().Be(50);
                // start time
                // time

                var extrusionMachine = machine as GetBlowFilmMachine_Machine_ExtrusionMachine;
                extrusionMachine.Should().NotBeNull();
                extrusionMachine!.ActualProcessValues.TwoSigma.LastValue.Should().Be(ExpectedTwoSigma);
                extrusionMachine.ActualProcessValues.TwoSigma.Unit.Should().Be("%");
                extrusionMachine.LineSpeed.Unit.Should().Be("m/min");
                extrusionMachine.LineSpeed.Value.Should().BeApproximately(ExpectedLineSpeed, precision: 0.01);
                extrusionMachine.ThroughputRate.Unit.Should().Be("kg/h");
                extrusionMachine.ThroughputRate.Value.Should().Be(ExpectedThroughputRate);
            },
            verifyQueryResponseAfterMachineSimulationIsStoppedFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetBlowFilmMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                var extrusionProducedJob = producedJob as GetBlowFilmMachine_Machine_ProducedJob_ExtrusionProducedJob;
                extrusionProducedJob.Should().NotBeNull();
                extrusionProducedJob!.GoodLength.Unit.Should().Be("m");
                extrusionProducedJob.GoodLength.Value.Should()
                    .BeApproximately(ExpectedExtrusionGoodLength, precision: 5.0);
                extrusionProducedJob.GoodWeight.Unit.Should().Be("kg");
                extrusionProducedJob.GoodWeight.Value.Should().BeApproximately(ExpectedGoodWeight, precision: 2.0);
                extrusionProducedJob.ScrapLength.Unit.Should().Be("m");
                extrusionProducedJob.ScrapLength.Value.Should().Be(0.0);
                extrusionProducedJob.ScrapWeight.Unit.Should().Be("kg");
                extrusionProducedJob.ScrapWeight.Value.Should().Be(0.0);
            });
    }

    [Fact]
    public async Task Should_Get_Queried_CastFilm_Data()
    {
        const string expectedMachineId = "EQ10311";

        await PerformTest(
            expectedMachineId,
            setupAction: () =>
            {
                // Arrange
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Extrusion.Customer, ExpectedCustomer);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Extrusion.JobSize, ExpectedJobSize);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Extrusion.Thickness, ExpectedThickness);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper, expectedMachineId, Constants.Paths.Extrusion.CastFilm.Width, ExpectedWidth);
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.BlowFilm.IsThicknessGaugeOn,
                    "true");
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.CastFilm.TwoSigma,
                    ExpectedTwoSigma);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.CastFilm.LineSpeed,
                    ExpectedLineSpeed);
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.ThroughputRate,
                    ExpectedThroughputRate);
            },
            verifyQueryResponseWhileMachineIsInProductionFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetCastFilmMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;
                machine.Department.Should().Be(MachineDepartment.Extrusion);
                machine.MachineFamily.Should().Be(MachineFamily.CastFilm);
                machine.MachineType.Should().Be("FILMEX");
                machine.MachineId.Should().Be(expectedMachineId);
                machine.Name.Should().Be(ExpectedMachineName);

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                producedJob.Customer.LastValue.Should().Be(ExpectedCustomer);
                producedJob.EndTime.Should().BeNull();
                producedJob.IsActive.Should().BeTrue();
                producedJob.JobId.Should().StartWith(ExpectedJob);
                producedJob.JobSize.LastValue.Should().Be(ExpectedJobSize);
                producedJob.JobSize.Unit.Should().Be("m");
                producedJob.MachineId.Should().Be(machine.MachineId);
                producedJob.ProductId.Should().Be(ExpectedProductId);
                // start time
                producedJob.UniqueId.Should().Be($"{producedJob.MachineId}_{producedJob.JobId}");

                var extrusionProducedJob = producedJob as GetCastFilmMachine_Machine_ProducedJob_ExtrusionProducedJob;
                extrusionProducedJob.Should().NotBeNull();

                var machineSettings = extrusionProducedJob!.MachineSettings;
                machineSettings.Thickness.Should().NotBeNull();
                machineSettings.Thickness.LastValue.Should().Be(ExpectedThickness);
                machineSettings.Thickness.Unit.Should().Be("µm");
                machineSettings.Width.Should().NotBeNull();
                machineSettings.Width.LastValue.Should().Be(ExpectedWidth);
                machineSettings.Width.Unit.Should().Be("mm");

                machine.ProductionStatus.Category.Should().Be(ProductionStatusCategory.Production);
                machine.ProductionStatus.Id.Should().Be(50);
                // start time
                // time

                var extrusionMachine = machine as GetCastFilmMachine_Machine_ExtrusionMachine;
                extrusionMachine.Should().NotBeNull();
                extrusionMachine!.ActualProcessValues.TwoSigma.LastValue.Should().Be(ExpectedTwoSigma);
                extrusionMachine.ActualProcessValues.TwoSigma.Unit.Should().Be("%");
                extrusionMachine.LineSpeed.Unit.Should().Be("m/min");
                extrusionMachine.LineSpeed.Value.Should().BeApproximately(ExpectedLineSpeed, precision: 0.01);
                extrusionMachine.ThroughputRate.Unit.Should().Be("kg/h");
                extrusionMachine.ThroughputRate.Value.Should().Be(ExpectedThroughputRate);
            },
            verifyQueryResponseAfterMachineSimulationIsStoppedFunc: async () =>
            {
                // Act
                var operationResult = await _frameworkApiClient.GetCastFilmMachine.ExecuteAsync();

                // Assert
                var result = ValidateAndGetResult(operationResult);

                using var _ = new AssertionScope();
                var machine = result!.Machine;

                machine.ProducedJob.Should().NotBeNull();
                var producedJob = machine.ProducedJob!;
                var extrusionProducedJob = producedJob as GetCastFilmMachine_Machine_ProducedJob_ExtrusionProducedJob;
                extrusionProducedJob.Should().NotBeNull();
                extrusionProducedJob!.GoodLength.Unit.Should().Be("m");
                extrusionProducedJob.GoodLength.Value.Should()
                    .BeApproximately(ExpectedExtrusionGoodLength, precision: 5.0);
                extrusionProducedJob.GoodWeight.Unit.Should().Be("kg");
                extrusionProducedJob.GoodWeight.Value.Should().BeApproximately(ExpectedGoodWeight, precision: 2.0);
                extrusionProducedJob.ScrapLength.Unit.Should().Be("m");
                extrusionProducedJob.ScrapLength.Value.Should().Be(0.0);
                extrusionProducedJob.ScrapWeight.Unit.Should().Be("kg");
                extrusionProducedJob.ScrapWeight.Value.Should().Be(0.0);
            });
    }

    [Fact]
    public async Task Should_Get_Receive_BlowFilm_Speed_Data()
    {
        const string expectedMachineId = "EQ10301";
        const int expectedAmountOfValuesInTrend = 8 * 60 + 1;

        await PerformTest(
            expectedMachineId,
            setupAction: () =>
            {
                // Arrange
                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.Customer,
                    ExpectedCustomer);

                _ = SetAndIncrementParameterContinuously(
                    expectedMachineId,
                    Constants.Paths.Extrusion.BlowFilm.LineSpeed,
                    ExpectedSpeed,
                    CancellationToken.None);

                _ = SetAndIncrementParameterContinuously(
                    expectedMachineId,
                    Constants.Paths.Extrusion.BlowFilm.Width,
                    ExpectedWidth,
                    CancellationToken.None,
                    100);
            },
            verifyQueryResponseWhileMachineIsInProductionFunc: async () =>
            {
                // Arrange
                using var _ = new AssertionScope();

                // Act - Query with trend of lineSpeed
                await Task.Delay(TimeSpan.FromMinutes(5));
                var queryOperationResult = await _frameworkApiClient.GetBlowFilmLineSpeedTrend.ExecuteAsync();

                // Assert - Query with trend of lineSpeed
                var queryResult = ValidateAndGetResult(queryOperationResult);
                var lineSpeedTrendValues = GetLineSpeedTrendValuesByQueryResponse(queryResult!).ToList();

                var expectedLineSpeedsOfQuery = Enumerable.Range(start: 1, count: 5)
                    .Select(delta => ExpectedSpeed + (double?)delta);

                lineSpeedTrendValues.Count.Should().Be(expectedAmountOfValuesInTrend);
                lineSpeedTrendValues.Should().ContainInConsecutiveOrder(expectedLineSpeedsOfQuery);
            },
            verifyQueryResponseAfterMachineSimulationIsStoppedFunc: async () =>
            {
                // Act
                await AdminApiHelper.EnsureApplicationStopped(_testOutputHelper, OpcUaForwarderContainerName);
                await Task.Delay(TimeSpan.FromSeconds(65));
                var queryOperationResult = await _frameworkApiClient.GetBlowFilmLineSpeedTrend.ExecuteAsync();

                // Assert
                using var _ = new AssertionScope();
                var queryResult = ValidateAndGetResult(queryOperationResult);
                var lineSpeedTrendValues = GetLineSpeedTrendValuesByQueryResponse(queryResult!).ToList();

                lineSpeedTrendValues.Count.Should().Be(expectedAmountOfValuesInTrend);
                var expectedLineSpeedsOfQuery = Enumerable.Range(start: 1, count: 5)
                    .Select(delta => ExpectedSpeed + (double?)delta);
                lineSpeedTrendValues.Should().ContainInConsecutiveOrder(expectedLineSpeedsOfQuery);
                lineSpeedTrendValues.Last().Should().BeNull();
                await AdminApiHelper.EnsureApplicationStarted(_testOutputHelper, OpcUaForwarderContainerName);
            });
    }

    [Fact]
    public async Task Should_Get_Queried_BlowFilm_Extrusion_Profile_Data()
    {
        const string expectedMachineId = "EQ10301";

        await PerformTest(
            expectedMachineId,
            setupAction: () =>
            {
                // Arrange
                var profileEntries = new List<float>();

                for (var i = 0; i < ExpectedDataPointsCount + 120; i++)
                {
                    profileEntries.Add(ExpectedDataPointsValue);
                }

                var controlElements = new List<float>() { 1f, 2f, 3f };

                MachineSimulationHelper.SetStringProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.Customer,
                    ExpectedCustomer);

                // Primary Profile
                MachineSimulationHelper.SetNumericArrayProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.PrimaryProfile,
                    profileEntries.ToArray());

                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.PrimaryProfileMeanValue,
                    ExpectedProfileMean);

                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.PrimaryProfileTwoSigma,
                    ExpectedTwoSigma);

                MachineSimulationHelper.SetNumericArrayProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.ControlElements,
                    controlElements.ToArray());

                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.ProfileControl,
                    1.0);

                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.BlowFilm.IsThicknessGaugeOn,
                    1);

                // MdoProfileA
                MachineSimulationHelper.SetNumericArrayProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.MdoProfileA,
                    profileEntries.ToArray());

                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.MdoProfileAMeanValue,
                    ExpectedProfileMean);

                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.MdoProfileATwoSigma,
                    ExpectedTwoSigma);

                // MdoProfileB
                MachineSimulationHelper.SetNumericArrayProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.MdoProfileB,
                    profileEntries.ToArray());

                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.MdoProfileBMeanValue,
                    ExpectedProfileMean);

                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.MdoProfileBTwoSigma,
                    ExpectedTwoSigma);
            },
            verifyQueryResponseWhileMachineIsInProductionFunc: async () =>
            {
                // Arrange
                using var _ = new AssertionScope();

                // Act
                await Task.Delay(TimeSpan.FromMinutes(1));
                var queryOperationResult = await _frameworkApiClient.GetBlowFilmExtrusionProfile.ExecuteAsync();

                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.WinderAContactDrive,
                    1.0
                );

                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    expectedMachineId,
                    Constants.Paths.Extrusion.ProfileControl,
                    4.0
                );

                await Task.Delay(TimeSpan.FromMinutes(1));
                var queryOperationResultAfterChange = await _frameworkApiClient.GetBlowFilmExtrusionProfile.ExecuteAsync();

                // Assert
                var queryResult = ValidateAndGetResult(queryOperationResult);
                var queryResultAfterChange = ValidateAndGetResult(queryOperationResultAfterChange);

                var machine = queryResult!.Machine;
                var machineAfterChange = queryResultAfterChange!.Machine;
                var extrusionMachine = machine as GetBlowFilmExtrusionProfile_Machine_ExtrusionMachine;
                var extrusionMachineAfterChange = machineAfterChange as GetBlowFilmExtrusionProfile_Machine_ExtrusionMachine;

                var mostRelevantProfile = extrusionMachine!.ActualProcessValues.ThicknessProfiles.MostRelevantProfile
                    as GetBlowFilmExtrusionProfile_Machine_ActualProcessValues_ThicknessProfiles_MostRelevantProfile_ExtrusionThicknessProfile;
                var primaryProfile = extrusionMachine.ActualProcessValues.ThicknessProfiles.PrimaryProfile
                    as GetBlowFilmExtrusionProfile_Machine_ActualProcessValues_ThicknessProfiles_PrimaryProfile_ExtrusionThicknessProfile;
                var mdoProfileA = extrusionMachine.ActualProcessValues.ThicknessProfiles.MdoProfileA
                    as GetBlowFilmExtrusionProfile_Machine_ActualProcessValues_ThicknessProfiles_MdoProfileA_ExtrusionThicknessProfile;
                var mdoProfileB = extrusionMachine.ActualProcessValues.ThicknessProfiles.MdoProfileB
                    as GetBlowFilmExtrusionProfile_Machine_ActualProcessValues_ThicknessProfiles_MdoProfileB_ExtrusionThicknessProfile;
                var mostRelevantProfileAfterChange = extrusionMachineAfterChange!.ActualProcessValues.ThicknessProfiles.MostRelevantProfile
                    as GetBlowFilmExtrusionProfile_Machine_ActualProcessValues_ThicknessProfiles_MostRelevantProfile_ExtrusionThicknessProfile;

                machine.MachineId.Should().Be(expectedMachineId);
                machine.Department.Should().Be(MachineDepartment.Extrusion);
                machine.MachineFamily.Should().Be(MachineFamily.BlowFilm);

                machineAfterChange.MachineId.Should().Be(expectedMachineId);
                machineAfterChange.Department.Should().Be(MachineDepartment.Extrusion);
                machineAfterChange.MachineFamily.Should().Be(MachineFamily.BlowFilm);

                mostRelevantProfile.Should().NotBeNull();
                mostRelevantProfile!.Type.Should().Be(ExtrusionThicknessMeasurementType.Primary);

                mostRelevantProfileAfterChange.Should().NotBeNull();
                mostRelevantProfileAfterChange!.Type.Should().Be(ExtrusionThicknessMeasurementType.MdoWinderA);

                primaryProfile.Should().NotBeNull();
                primaryProfile!.Type.Should().Be(ExtrusionThicknessMeasurementType.Primary);
                primaryProfile.DataPointsCount.Should().Be(ExpectedDataPointsCount);
                primaryProfile.DataPoints.Should().NotBeNull();
                primaryProfile.DataPoints![0].Value.Should().BeApproximately(0, 0.01);
                primaryProfile.MeanValue!.Value.Should().Be(ExpectedProfileMean);
                primaryProfile.MeanValue.Unit.Should().Be("µm");
                primaryProfile.XAxisUnit.Should().Be("°");
                primaryProfile.TwoSigma!.Value.Should().Be(ExpectedTwoSigma);
                primaryProfile.TwoSigma.Unit.Should().Be("%");
                primaryProfile.ControlElements!.Count.Should().Be(3);
                primaryProfile.ControlElements[0].Value.Should().Be(1);
                primaryProfile.IsControllerOn.Should().Be(true);
                primaryProfile.Timestamp.Should().NotBeNull();

                mdoProfileA.Should().NotBeNull();
                mdoProfileA!.Type.Should().Be(ExtrusionThicknessMeasurementType.MdoWinderA);
                mdoProfileA.DataPointsCount.Should().Be(ExpectedDataPointsCount);
                mdoProfileA.DataPoints.Should().NotBeNull();
                mdoProfileA.DataPoints![0].Value.Should().BeApproximately(0, 0.01);
                mdoProfileA.MeanValue!.Value.Should().Be(ExpectedProfileMean);
                mdoProfileA.MeanValue.Unit.Should().Be("µm");
                mdoProfileA.XAxisUnit.Should().Be("°");
                mdoProfileA.TwoSigma!.Value.Should().Be(ExpectedTwoSigma);
                mdoProfileA.TwoSigma.Unit.Should().Be("%");
                mdoProfileA.ControlElements!.Should().BeNull();
                mdoProfileA.IsControllerOn.Should().Be(false);
                mdoProfileA.Timestamp.Should().NotBeNull();

                mdoProfileB.Should().NotBeNull();
                mdoProfileB!.Type.Should().Be(ExtrusionThicknessMeasurementType.MdoWinderB);
                mdoProfileB.DataPointsCount.Should().Be(ExpectedDataPointsCount);
                mdoProfileB.DataPoints.Should().NotBeNull();
                mdoProfileB.DataPoints![0].Value.Should().BeApproximately(0, 0.01);
                mdoProfileB.MeanValue!.Value.Should().Be(ExpectedProfileMean);
                mdoProfileB.MeanValue.Unit.Should().Be("µm");
                mdoProfileB.XAxisUnit.Should().Be("°");
                mdoProfileB.TwoSigma!.Value.Should().Be(ExpectedTwoSigma);
                mdoProfileB.TwoSigma.Unit.Should().Be("%");
                mdoProfileA.ControlElements!.Should().BeNull();
                mdoProfileB.IsControllerOn.Should().Be(false);
                mdoProfileB.Timestamp.Should().NotBeNull();
            },
            verifyQueryResponseAfterMachineSimulationIsStoppedFunc: async () =>
            {
                // Act
                await AdminApiHelper.EnsureApplicationStopped(_testOutputHelper, OpcUaForwarderContainerName);
                await Task.Delay(TimeSpan.FromSeconds(65));
                var queryOperationResult = await _frameworkApiClient.GetBlowFilmLineSpeedTrend.ExecuteAsync();

                // Assert
                using var _ = new AssertionScope();
                var queryResult = ValidateAndGetResult(queryOperationResult);

                await AdminApiHelper.EnsureApplicationStarted(_testOutputHelper, OpcUaForwarderContainerName);
            });
    }

    private async Task PerformTest(
        string machineId,
        Action setupAction,
        Func<Task> verifyQueryResponseWhileMachineIsInProductionFunc,
        Func<Task> verifyQueryResponseAfterMachineSimulationIsStoppedFunc)
    {
        try
        {
            // Arrange
            MachineSimulationHelper.EnsureMachineSimulationIsReady(_testOutputHelper, machineId);
            await AdminApiHelper.WaitUntilSnapshooterRoutineActive(_testOutputHelper, machineId);

            // We use stop here, to be sure that machine is not already started (e.g. testing on ft1).
            MachineSimulationHelper.Stop(_testOutputHelper, machineId);
            MachineSimulationHelper.ChangeJob(_testOutputHelper, machineId, ExpectedJob, ExpectedProductId);

            // Simulate a setup phase
            // Why? Answer: We need a clear cut when it comes to the values we set. They are sometimes assigned to the setup
            // phase when the setup phase is really short although we already transitioned to the production phase
            var simulateSetupPhaseTimeSpan = TimeSpan.FromSeconds(30);
            _testOutputHelper.WriteLineWithTimestamp($"Simulating a setup phase for {simulateSetupPhaseTimeSpan.TotalSeconds}s...");
            await Task.Delay(simulateSetupPhaseTimeSpan);
            _testOutputHelper.WriteLineWithTimestamp($"Simulated a setup phase for {simulateSetupPhaseTimeSpan.TotalSeconds}s.");

            setupAction();

            _testOutputHelper.WriteLineWithTimestamp(
                $"Simulating a production phase for {SimulateProductionPhaseTimeSpan.TotalSeconds}s...");
            MachineSimulationHelper.Start(_testOutputHelper, machineId, ExpectedSpeed);
            await Task.Delay(SimulateProductionPhaseTimeSpan);

            // Act, Assert - during production -
            await verifyQueryResponseWhileMachineIsInProductionFunc();

            MachineSimulationHelper.Stop(_testOutputHelper, machineId);
            _testOutputHelper.WriteLineWithTimestamp(
                $"Simulated a production phase for {SimulateProductionPhaseTimeSpan.TotalSeconds}s.");

            var waitForNewSnapshotTimeSpan = TimeSpan.FromSeconds(65);
            _testOutputHelper.WriteLineWithTimestamp(
                $"Waiting {waitForNewSnapshotTimeSpan.TotalSeconds}s for a new snapshot to be created...");
            await Task.Delay(waitForNewSnapshotTimeSpan);
            _testOutputHelper.WriteLineWithTimestamp(
                $"Waited {waitForNewSnapshotTimeSpan.TotalSeconds}s for a new snapshot to be created.");

            // Act, Assert - when machine shutdown -
            await verifyQueryResponseAfterMachineSimulationIsStoppedFunc();
        }
        catch (Exception ex)
        {
            MachineSimulationHelper.Stop(_testOutputHelper, machineId);
            AdminApiHelper.SaveAllRubyApplicationLogsAsCIJobArtifact(_testOutputHelper);
            _testOutputHelper.WriteLineWithTimestamp($"Exception: {ex.Message}");
            await AdminApiHelper.EnsureApplicationStarted(_testOutputHelper, "OPC UA Forwarder");
            throw;
        }
    }

    private static T? ValidateAndGetResult<T>(IOperationResult<T> operationResult) where T : class
    {
        if (operationResult.Errors.Any())
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{operationResult.Errors.Count} error(s) occurred when executing the query:");

            foreach (var error in operationResult.Errors)
            {
                stringBuilder.AppendLine($"- Message: {error.Message}");

                if (error.Path is not null)
                {
                    stringBuilder.AppendLine($"  Path: {string.Join('.', error.Path)}");
                }

                if (error.Extensions is null)
                {
                    continue;
                }

                if (error.Extensions.TryGetValue("message", out var message) && message is not null)
                {
                    stringBuilder.AppendLine($"  Extensions.Message: {message}");
                }

                if (error.Extensions.TryGetValue("stackTrace", out var stackTrace) && stackTrace is not null)
                {
                    stringBuilder.AppendLine($"  Extensions.StackTrace: {stackTrace}");
                }
            }

            throw new Exception(message: stringBuilder.ToString());
        }

        var result = operationResult.Data;
        result.Should().NotBeNull();

        return result;
    }

    private Task SetAndIncrementParameterContinuously(
        string machineId,
        string endOfVariablePath,
        double startValue,
        CancellationToken cancellationToken,
        int iterateBy = 1)
    {
        return Task.Run(async () =>
        {
            var i = 0.00;
            while (!cancellationToken.IsCancellationRequested)
            {
                _testOutputHelper.WriteLineWithTimestamp($"Set parameter {endOfVariablePath} to {startValue} + {i}");
                MachineSimulationHelper.SetNumericProcessVariable(
                    _testOutputHelper,
                    machineId,
                    endOfVariablePath,
                    startValue + i);

                i += iterateBy;
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }, cancellationToken);
    }

    private static IEnumerable<double?> GetLineSpeedTrendValuesByQueryResponse(IGetBlowFilmLineSpeedTrendResult queryResult)
    {
        var machine = queryResult.Machine;
        var extrusionMachine = machine as GetBlowFilmLineSpeedTrend_Machine_ExtrusionMachine;
        var lineSpeedTrend = extrusionMachine!.LineSpeed
            as GetBlowFilmLineSpeedTrend_Machine_LineSpeed_NumericSnapshotValueAndTrend;
        return lineSpeedTrend!.TrendOfLast8Hours!.Select(i => i.Value);
    }
}