using System;
using System.Collections.Generic;
using System.Linq;
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

[Collection("Slow")]
[TestCaseOrderer(
    ordererTypeName: "FrameworkAPI.E2E.Test.Helper.OrderTestCasesByAlphabet",
    ordererAssemblyName: "FrameworkAPI.E2E.Test")]
public class ProductGroupsE2ETests
{
    private const string HostnameUri = "lx64ispft4.wuh-intern.de";
    private const string MachineSimulationContainerName = "Machine Simulation";
    private const string ExpectedPrefix = "ProductGroupsE2ETests";
    private const string BottomerMachineId = "EQ10221";
    private const string TuberMachineId = "EQ10211";

    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IFrameworkAPIClient _frameworkApiClient;

    private abstract record JobStep(TimeSpan WaitAfterInMinutes);

    private record JobStepStart(double Speed, TimeSpan WaitAfterInMinutes) : JobStep(WaitAfterInMinutes);
    private record JobStepStop(TimeSpan WaitAfterInMinutes) : JobStep(WaitAfterInMinutes);

    private record JobStepChangeNumericVariable(string MachineId, string EndOfVariablePath, double Value, TimeSpan WaitAfterInMinutes) : JobStep(WaitAfterInMinutes);

    public ProductGroupsE2ETests(ITestOutputHelper testOutputHelper)
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
    public async Task Initial_ProductGroup_Migration_Creates_ProductGroups_And_Sets_ProductGroup_On_Matching_StandardJobKpis()
    {
        // Arrange
        await AdminApiHelper.EnsureApplicationStarted(_testOutputHelper, MachineSimulationContainerName);
        MachineSimulationHelper.EnsureMachineSimulationIsReady(_testOutputHelper, BottomerMachineId);
        MachineSimulationHelper.EnsureMachineSimulationIsReady(_testOutputHelper, TuberMachineId);

        // Act
        var executedRuns = new List<(string BottomerJobId, string TuberJobId, string Product)>
        {
            await RunJobForBottomerAndTuber(
                new JobStepChangeNumericVariable(BottomerMachineId, Constants.Paths.PaperSack.Bottomer.SackWidth, 695.0, TimeSpan.FromSeconds(10)),
                new JobStepStart(250, TimeSpan.FromMinutes(1)),
                new JobStepStop(TimeSpan.FromMinutes(1)),
                new JobStepStart(280, TimeSpan.FromMinutes(3)),
                new JobStepStop(TimeSpan.FromMinutes(2)),
                new JobStepStart(260, TimeSpan.FromMinutes(3))
            ),
            await RunJobForBottomerAndTuber(
                new JobStepChangeNumericVariable(BottomerMachineId, Constants.Paths.PaperSack.Bottomer.SackWidth, 510.0, TimeSpan.FromSeconds(10)),
                new JobStepChangeNumericVariable(BottomerMachineId, Constants.Paths.PaperSack.Bottomer.ValveUnit1IsActive, 1.0, TimeSpan.Zero),
                new JobStepChangeNumericVariable(BottomerMachineId, Constants.Paths.PaperSack.Bottomer.ValveUnit1Layers, 8, TimeSpan.Zero),
                new JobStepStop(TimeSpan.FromMinutes(1)),
                new JobStepStart(250, TimeSpan.FromMinutes(10)),
                new JobStepStop(TimeSpan.FromMinutes(1))
            ),
            await RunJobForBottomerAndTuber(
                new JobStepStart(245, TimeSpan.FromMinutes(8))
            ),
            await RunJobForBottomerAndTuber(
                new JobStepChangeNumericVariable(BottomerMachineId, Constants.Paths.PaperSack.Bottomer.SackWidth, 755.0, TimeSpan.FromSeconds(10)),
                new JobStepChangeNumericVariable(BottomerMachineId, Constants.Paths.PaperSack.Bottomer.ValveUnit1IsActive, 0.0, TimeSpan.Zero),
                new JobStepStart(250, TimeSpan.FromMinutes(1)),
                new JobStepStart(280, TimeSpan.FromMinutes(1)),
                new JobStepStop(TimeSpan.FromMinutes(2)),
                new JobStepStart(300, TimeSpan.FromMinutes(5))
            ),
        };

        // Assert
        var productsByProductGroupIds = new Dictionary<string, List<string>>();
        foreach (var (bottomerJobId, tuberJobId, product) in executedRuns)
        {
            var productGroupId = await AssertProducedJob(bottomerJobId, tuberJobId, product);

            if (!productsByProductGroupIds.ContainsKey(productGroupId))
                productsByProductGroupIds[productGroupId] = [];

            productsByProductGroupIds[productGroupId].Add(product);
        }

        foreach (var (productGroupId, products) in productsByProductGroupIds)
            await AssertProductGroup(productGroupId, products);
    }

    private async Task<(string BottomerJobId, string TuberJobId, string Product)> RunJobForBottomerAndTuber(params JobStep[] steps)
    {
        var suffix = DateTime.UtcNow.ToString("O");
        var bottomerJobId = $"{ExpectedPrefix}BottomerJob{suffix}";
        var tuberJobId = $"{ExpectedPrefix}TuberJob{suffix}";
        var product = $"ProductGroupsE2ETestsProduct{suffix}";

        MachineSimulationHelper.ChangeJob(_testOutputHelper, BottomerMachineId, bottomerJobId, product);
        MachineSimulationHelper.ChangeJob(_testOutputHelper, TuberMachineId, tuberJobId, product);
        await Task.Delay(TimeSpan.FromSeconds(30));

        foreach (var step in steps)
        {
            switch (step)
            {
                case JobStepStart jobStepStart:
                    MachineSimulationHelper.Start(_testOutputHelper, TuberMachineId, jobStepStart.Speed);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    MachineSimulationHelper.Start(_testOutputHelper, BottomerMachineId, jobStepStart.Speed);
                    break;

                case JobStepStop:
                    MachineSimulationHelper.Stop(_testOutputHelper, TuberMachineId);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    MachineSimulationHelper.Stop(_testOutputHelper, BottomerMachineId);
                    break;

                case JobStepChangeNumericVariable jobStepChangeNumericVariable:
                    MachineSimulationHelper.SetNumericProcessVariable(_testOutputHelper, jobStepChangeNumericVariable.MachineId, jobStepChangeNumericVariable.EndOfVariablePath, jobStepChangeNumericVariable.Value);
                    break;
            }

            await Task.Delay(step.WaitAfterInMinutes);
        }

        MachineSimulationHelper.Stop(_testOutputHelper, BottomerMachineId);
        MachineSimulationHelper.Stop(_testOutputHelper, TuberMachineId);
        await Task.Delay(TimeSpan.FromMinutes(1));
        MachineSimulationHelper.ChangeJob(_testOutputHelper, BottomerMachineId, $"{ExpectedPrefix}BottomerTriggeredChange{suffix}", $"TestProduct{suffix}");
        MachineSimulationHelper.ChangeJob(_testOutputHelper, TuberMachineId, $"{ExpectedPrefix}TuberTriggeredChange{suffix}", $"TestProduct{suffix}");
        await Task.Delay(TimeSpan.FromMinutes(1));

        return (bottomerJobId, tuberJobId, product);
    }

    private async Task<string> AssertProducedJob(string bottomerJobId, string tuberJobId, string product)
    {
        var getBottomerPaperSackProducedJob = await _frameworkApiClient.GetPaperSackProducedJob.ExecuteAsync(bottomerJobId, BottomerMachineId);
        getBottomerPaperSackProducedJob.Errors.Should().BeEmpty();
        getBottomerPaperSackProducedJob.Data.Should().NotBeNull();

        getBottomerPaperSackProducedJob.Data!.ProducedJob.Should().BeOfType<GetPaperSackProducedJob_ProducedJob_PaperSackProducedJob>();
        var bottomerPaperSackProducedJob = getBottomerPaperSackProducedJob.Data!.ProducedJob.As<GetPaperSackProducedJob_ProducedJob_PaperSackProducedJob>();
        getBottomerPaperSackProducedJob.Data!.ProducedJob.MachineId.Should().Be(BottomerMachineId);
        getBottomerPaperSackProducedJob.Data!.ProducedJob.JobId.Should().Be(bottomerJobId);
        getBottomerPaperSackProducedJob.Data!.ProducedJob.ProductId.Should().Be(product);
        bottomerPaperSackProducedJob.ProductGroup.Should().NotBeNull();
        bottomerPaperSackProducedJob.EndTime.Should().NotBeNull();

        var getTuberPaperSackProducedJob = await _frameworkApiClient.GetPaperSackProducedJob.ExecuteAsync(tuberJobId, TuberMachineId);
        getTuberPaperSackProducedJob.Errors.Should().BeEmpty();
        getTuberPaperSackProducedJob.Data.Should().NotBeNull();

        getTuberPaperSackProducedJob.Data!.ProducedJob.Should().BeOfType<GetPaperSackProducedJob_ProducedJob_PaperSackProducedJob>();
        var tuberPaperSackProducedJob = getTuberPaperSackProducedJob.Data!.ProducedJob.As<GetPaperSackProducedJob_ProducedJob_PaperSackProducedJob>();
        tuberPaperSackProducedJob.MachineId.Should().Be(TuberMachineId);
        tuberPaperSackProducedJob.JobId.Should().Be(tuberJobId);
        tuberPaperSackProducedJob.ProductId.Should().Be(product);
        tuberPaperSackProducedJob.ProductGroup.Should().NotBeNull();
        tuberPaperSackProducedJob.EndTime.Should().NotBeNull();

        bottomerPaperSackProducedJob.ProductGroup!.Id.Should().Be(tuberPaperSackProducedJob.ProductGroup!.Id);

        return bottomerPaperSackProducedJob.ProductGroup.Id;
    }

    private async Task AssertProductGroup(string productGroupId, List<string> expectedProducts)
    {
        var getPaperSackProductGroupsResult = await _frameworkApiClient.GetPaperSackProductGroups.ExecuteAsync();
        getPaperSackProductGroupsResult.Errors.Should().BeEmpty();
        getPaperSackProductGroupsResult.Data.Should().NotBeNull();
        getPaperSackProductGroupsResult.Data!.PaperSackProductGroups.Should().NotBeNull();

        var paperSackProductGroup = getPaperSackProductGroupsResult.Data!.PaperSackProductGroups!.Items!.FirstOrDefault(x => x.Id == productGroupId);
        paperSackProductGroup.Should().NotBeNull();
        paperSackProductGroup!.ProductIds.Should().Contain(expectedProducts);
    }
}