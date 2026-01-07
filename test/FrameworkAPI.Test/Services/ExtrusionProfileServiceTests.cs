using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Schema.Machine.ActualProcessValues;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.Services.Helpers;
using Moq;
using Newtonsoft.Json;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using Xunit;
using VariableUnits = WuH.Ruby.MetaDataHandler.Client.VariableUnits;

namespace FrameworkAPI.Test.Services;

public class ExtrusionProfileServiceTests
{
    private readonly Mock<IProcessDataService> _processDataServiceMock = new();
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<IMetaDataHandlerHttpClient> _metaDataHandlerHttpClientMock = new();
    private readonly Mock<IProcessDataCachingService> _processDataCachingServiceMock = new();
    private readonly ProcessDataByTimestampBatchDataLoader _processDataBatchDataLoader;
    private readonly MachineMetaDataBatchDataLoader _machineMetaDataBatchDataLoader;
    private readonly LatestProcessDataCacheDataLoader _latestProcessDataCacheDataLoader;
    private readonly ExtrusionProfileService _subject;
    private readonly string _machineId = "EQ12345";
    private readonly MachineFamily _machineFamily = MachineFamily.BlowFilm;
    private readonly DateTime _timestamp = DateTime.UnixEpoch.AddHours(2);
    private readonly VariableUnits _variableUnits = new()
    {
        Si = new VariableUnits.UnitWithCoefficient
        {
            Unit = "Test"
        }
    };

    public ExtrusionProfileServiceTests()
    {
        _processDataBatchDataLoader = new ProcessDataByTimestampBatchDataLoader(
            _processDataReaderHttpClientMock.Object,
            new DelayedBatchScheduler()
        );

        _machineMetaDataBatchDataLoader = new MachineMetaDataBatchDataLoader(
            _metaDataHandlerHttpClientMock.Object,
            new DelayedBatchScheduler()
        );

        _latestProcessDataCacheDataLoader = new LatestProcessDataCacheDataLoader(
            _processDataCachingServiceMock.Object
        );

        _subject = new ExtrusionProfileService(_processDataServiceMock.Object);
    }

    [Fact]
    public async Task GetMostRelevantProfile_Should_Return_Primary_Profile()
    {
        // Arrange
        SetupExtrusionProfileMocks();

        // Act
        var result = await _subject.GetMostRelevantProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ExtrusionThicknessMeasurementType.Primary);
    }

    [Fact]
    public async Task GetMostRelevantProfile_Should_Return_Primary_Profile_Because_ProfileControl_Is_Null()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.ProfileControl);

        // Act
        var result = await _subject.GetMostRelevantProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ExtrusionThicknessMeasurementType.Primary);
    }

    [Fact]
    public async Task GetMostRelevantProfile_Should_Return_Primary_Profile_Because_WinderAContactDrive_Is_Null()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.WinderAContactDrive);

        // Act
        var result = await _subject.GetMostRelevantProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ExtrusionThicknessMeasurementType.Primary);
    }

    [Fact]
    public async Task GetMostRelevantProfile_Should_Return_Mdo_Profile_A()
    {
        // Arrange
        SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType.MdoWinderA);

        // Act
        var result = await _subject.GetMostRelevantProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ExtrusionThicknessMeasurementType.MdoWinderA);
    }

    [Fact]
    public async Task GetMostRelevantProfile_Should_Return_Mdo_Profile_B()
    {
        // Arrange
        SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType.MdoWinderB);
        SetupProcessDataServiceMock(Constants.LastPartOfPath.WinderAContactDrive, 0.0);

        // Act
        var result = await _subject.GetMostRelevantProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ExtrusionThicknessMeasurementType.MdoWinderB);
    }

    [Fact]
    public async Task GetMostRelevantProfile_Should_Return_Primary_Profile_For_CastFilm_Machine()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.ProfileControl, "false");

        // Act
        var result = await _subject.GetMostRelevantProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ExtrusionThicknessMeasurementType.Primary);
    }

    [Fact]
    public async Task GetMostRelevantProfile_Should_Return_Primary_Profile_For_CastFilm_Machine_With_ProfileControl_True()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.ProfileControl, "true");

        // Act
        var result = await _subject.GetMostRelevantProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ExtrusionThicknessMeasurementType.Primary);
    }

    [Fact]
    public async Task GetMostRelevantProfile_Should_Return_Primary_Profile_For_CastFilm_Machine_With_ProfileControl_False()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.ProfileControl, "false");

        // Act
        var result = await _subject.GetMostRelevantProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(ExtrusionThicknessMeasurementType.Primary);
    }

    [Fact]
    public async Task GetProfile_For_Primary_Profile()
    {
        // Arrange
        SetupExtrusionProfileMocks();

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        var meanValue = await result!.MeanValue!.Value(CancellationToken.None);
        var meanUnit = await result.MeanValue.Unit(CancellationToken.None);
        var twoSigmaValue = await result.TwoSigma!.Value(CancellationToken.None);
        var twoSigmaUnit = await result.TwoSigma.Unit(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.XAxisUnit.Should().Be("°");
        result.DataPoints.Should().HaveCount(480);
        result.DataPoints![0].Should().BeApproximately(-99.0, 0.01);
        result.DataPointsCount.Should().Be(480);
        meanValue.Should().NotBeNull();
        meanValue.Should().BeApproximately(100.0, 0.01);
        meanUnit.Should().NotBeNull();
        meanUnit.Should().Be("Test");
        twoSigmaValue.Should().NotBeNull();
        twoSigmaValue.Should().BeApproximately(200, 0.01);
        twoSigmaUnit.Should().NotBeNull();
        twoSigmaUnit.Should().Be("Test");
        result.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(true);
        result.ControlElements.Should().NotBeNull();
        result.ControlElements.Should().HaveCount(3);
        result.Timestamp.Should().Be(_timestamp);
    }

    [Fact]
    public async Task GetProfile_For_Mdo_Profile_A()
    {
        // Arrange
        SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType.MdoWinderA);

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.MdoWinderA,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        var meanUnit = await result!.MeanValue!.Unit(CancellationToken.None);
        var meanValue = await result.MeanValue.Value(CancellationToken.None);
        var twoSigmaValue = await result.TwoSigma!.Value(CancellationToken.None);
        var twoSigmaUnit = await result.TwoSigma!.Unit(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DataPoints.Should().HaveCount(480);
        result.DataPoints![0].Should().BeApproximately(-99.0, 0.01);
        result.DataPointsCount.Should().Be(480);
        meanValue.Should().NotBeNull();
        meanValue.Should().BeApproximately(100.0, 0.01);
        meanUnit.Should().NotBeNull();
        twoSigmaValue.Should().NotBeNull();
        twoSigmaValue.Should().BeApproximately(200, 0.01);
        twoSigmaUnit.Should().NotBeNull();
        result.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(true);
        result.ControlElements.Should().BeNull();
        result.Timestamp.Should().Be(_timestamp);
    }

    [Fact]
    public async Task GetProfile_For_Mdo_Profile_B()
    {
        // Arrange
        SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType.MdoWinderB);

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.MdoWinderB,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        var meanUnit = await result!.MeanValue!.Unit(CancellationToken.None);
        var meanValue = await result.MeanValue.Value(CancellationToken.None);
        var twoSigmaValue = await result.TwoSigma!.Value(CancellationToken.None);
        var twoSigmaUnit = await result.TwoSigma.Unit(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DataPoints.Should().HaveCount(480);
        result.DataPoints![0].Should().BeApproximately(-99.0, 0.01);
        result.DataPointsCount.Should().Be(480);
        meanValue.Should().NotBeNull();
        meanValue.Should().BeApproximately(100.0, 0.01);
        meanUnit.Should().NotBeNull();
        twoSigmaValue.Should().NotBeNull();
        twoSigmaValue.Should().BeApproximately(200, 0.01);
        twoSigmaUnit.Should().NotBeNull();
        result.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(true);
        result.ControlElements.Should().BeNull();
        result.Timestamp.Should().Be(_timestamp);
    }

    [Fact]
    public async Task GetProfile_ProfileControl_Is_Int64_Instead_Of_Double()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.ProfileControl, (long)1);

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(true);
        result.Timestamp.Should().Be(_timestamp);
    }

    [Fact]
    public async Task GetProfile_X_Axis_Unit_For_Cast_Film_Machine()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        // @TODO

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            MachineFamily.CastFilm,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.XAxisUnit.Should().Be("");
        result.Timestamp.Should().Be(_timestamp);
    }

    [Fact]
    public async Task GetProfile_TwoSigma_Unit_Is_Different()
    {
        // Arrange
        var unit = new VariableUnits()
        {
            Si = new VariableUnits.UnitWithCoefficient
            {
                Unit = "OtherUnit"
            }
        };

        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.PrimaryProfileTwoSigma, unit: unit);

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        var twoSigmaUnit = await result!.TwoSigma!.Unit(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        twoSigmaUnit.Should().NotBeNull();
        twoSigmaUnit.Should().Be("OtherUnit");
    }

    [Fact]
    public async Task GetProfile_For_Primary_Profile_With_Two_Sigma_As_String()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.PrimaryProfileTwoSigma, "200", _variableUnits);

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        var meanUnit = await result!.MeanValue!.Unit(CancellationToken.None);
        var meanValue = await result.MeanValue.Value(CancellationToken.None);
        var twoSigmaValue = await result.TwoSigma!.Value(CancellationToken.None);
        var twoSigmaUnit = await result.TwoSigma.Unit(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DataPoints.Should().HaveCount(480);
        result.DataPoints![0].Should().BeApproximately(-99.0, 0.01);
        result.DataPointsCount.Should().Be(480);
        meanValue.Should().NotBeNull();
        meanValue.Should().BeApproximately(100.0, 0.01);
        meanUnit.Should().NotBeNull();
        twoSigmaValue.Should().NotBeNull();
        twoSigmaValue.Should().BeApproximately(200, 0.01);
        twoSigmaUnit.Should().NotBeNull();
        result.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(true);
        result.ControlElements.Should().NotBeNull();
        result.ControlElements.Should().HaveCount(3);
        result.Timestamp.Should().Be(_timestamp);
    }

    [Fact]
    public async Task GetProfile_For_Primary_Profile_With_DataPoints_Null()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.PrimaryProfile);
        SetupProcessDataServiceMock(Constants.LastPartOfPath.PrimaryProfileMeanValue);

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        var meanUnit = await result!.MeanValue!.Unit(CancellationToken.None);
        var meanValue = await result.MeanValue.Value(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DataPoints.Should().NotBeNull();
        result.DataPoints!.Count.Should().Be(0);
        meanValue.Should().BeNull();
        meanUnit.Should().BeNull();
        result.Timestamp.Should().Be(_timestamp);
    }

    [Fact]
    public async Task GetProfile_For_Primary_Profile_With_TwoSigma_Null()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.PrimaryProfileTwoSigma);

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        var meanUnit = await result!.MeanValue!.Unit(CancellationToken.None);
        var meanValue = await result.MeanValue.Value(CancellationToken.None);
        var twoSigmaValue = await result.TwoSigma!.Value(CancellationToken.None);
        var twoSigmaUnit = await result.TwoSigma.Unit(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DataPoints.Should().HaveCount(480);
        meanValue.Should().NotBeNull();
        meanValue.Should().BeApproximately(100.0, 0.01);
        meanUnit.Should().NotBeNull();
        twoSigmaValue.Should().BeNull();
        twoSigmaUnit.Should().BeNull();
    }

    [Fact]
    public async Task GetProfile_For_Primary_Profile_With_ControlElements_As_Array()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.ControlElements, new double[] { 1, 2, 3 });

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        var meanUnit = await result!.MeanValue!.Unit(CancellationToken.None);
        var meanValue = await result.MeanValue.Value(CancellationToken.None);
        var twoSigmaValue = await result.TwoSigma!.Value(CancellationToken.None);
        var twoSigmaUnit = await result.TwoSigma.Unit(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DataPoints.Should().HaveCount(480);
        result.DataPoints![0].Should().BeApproximately(-99.0, 0.01);
        result.DataPointsCount.Should().Be(480);
        meanValue.Should().NotBeNull();
        meanValue.Should().BeApproximately(100.0, 0.01);
        meanUnit.Should().NotBeNull();
        twoSigmaValue.Should().NotBeNull();
        twoSigmaValue.Should().BeApproximately(200, 0.01);
        twoSigmaUnit.Should().NotBeNull();
        result.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(true);
        result.ControlElements.Should().NotBeNull();
        result.ControlElements.Should().HaveCount(3);
        result.Timestamp.Should().Be(_timestamp);
    }

    [Fact]
    public async Task GetProfile_Controller_Off_For_Primary_Profile()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        MockIsControllerOn(state: false);

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(false);
    }

    [Fact]
    public async Task GetProfile_Controller_Off_For_Mdo_Profile_A()
    {
        // Arrange
        SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType.MdoWinderA);
        MockIsControllerOn(ExtrusionThicknessMeasurementType.MdoWinderA, false);

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.MdoWinderA,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(false);
    }

    [Fact]
    public async Task GetProfile_Controller_Off_For_Mdo_Profile_B()
    {
        // Arrange
        SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType.MdoWinderB);
        MockIsControllerOn(ExtrusionThicknessMeasurementType.MdoWinderB, false);

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.MdoWinderB,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(false);
    }

    [Fact]
    public async Task GetProfile_Controller_On_For_CastFilm_Machine()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.ProfileControl, "true");

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            MachineFamily.CastFilm,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(true);
    }

    [Fact]
    public async Task GetProfile_Controller_Off_For_CastFilm_Machine()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.ProfileControl, "false");

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            MachineFamily.CastFilm,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(false);
    }

    [Fact]
    public async Task GetProfile_Controller_Off_For_CastFilm_Machine_Short_Circuit()
    {
        // Arrange
        SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType.MdoWinderA);
        SetupProcessDataServiceMock(Constants.LastPartOfPath.ProfileControl, "false");

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.MdoWinderA,
            _machineId,
            MachineFamily.CastFilm,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsControllerOn.Should().NotBeNull();
        result.IsControllerOn.Should().Be(false);
    }

    [Fact]
    public async Task GetProfile_Downsamples_Entries()
    {
        // Arrange
        var profileEntriesMock = new List<double>();

        for (var i = 0; i < 500; i++)
        {
            profileEntriesMock.Add(100);
        }

        SetupExtrusionProfileMocks();
        MockDataPoints(entryList: profileEntriesMock);

        // Act
        var result = await _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        var numericValueUnit = await result!.MeanValue!.Unit(CancellationToken.None);
        var numericValueValue = await result.MeanValue.Value(CancellationToken.None);

        // Assert
        result.MeanValue.Should().NotBeNull();
        numericValueUnit.Should().NotBeNull();
        numericValueValue.Should().NotBeNull();
        result.DataPoints.Should().HaveCount(480);
        result.DataPointsCount.Should().Be(480);
    }

    [Fact]
    public async Task GetProfile_Gets_Exception_While_Retrieving_DataPoints()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        MockDataPoints(exception: new InternalServiceException());

        // Act
        var action = () => _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetProfile_Gets_Exception_While_Retrieving_Mean_Value()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        MockMeanValue(exception: new InternalServiceException());

        // Act
        var action = () => _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetProfile_Gets_Exception_While_Retrieving_TwoSigma_Value()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        MockTwoSigma(exception: new InternalServiceException());

        // Act
        var action = () => _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetProfile_Gets_Exception_While_Retrieving_IsControllerOn_Value()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        MockIsControllerOn(exception: new InternalServiceException());

        // Act
        var action = () => _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetProfile_Gets_Exception_While_Retrieving_ControlElements_Value()
    {
        // Arrange
        SetupExtrusionProfileMocks();
        MockControlElements(new InternalServiceException());

        // Act
        var action = () => _subject.GetProfile(
            _processDataBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            _machineId,
            _machineFamily,
            _timestamp,
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    private void SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType profileType = ExtrusionThicknessMeasurementType.Primary)
    {
        MockDataPoints(profileType);
        MockMeanValue(profileType);
        MockTwoSigma(profileType);
        MockIsControllerOn(profileType);

        if (profileType == ExtrusionThicknessMeasurementType.Primary)
        {
            MockControlElements();
        }
    }

    private void MockDataPoints(ExtrusionThicknessMeasurementType profileType = ExtrusionThicknessMeasurementType.Primary, List<double>? entryList = null, Exception? exception = null)
    {
        var path = profileType switch
        {
            ExtrusionThicknessMeasurementType.MdoWinderA => Constants.LastPartOfPath.MdoProfileA,
            ExtrusionThicknessMeasurementType.MdoWinderB => Constants.LastPartOfPath.MdoProfileB,
            _ => Constants.LastPartOfPath.PrimaryProfile,
        };

        if (exception is not null)
        {
            SetupProcessDataServiceMock(path, exception: exception);
            return;
        }

        var profileEntries = entryList ?? Enumerable.Range(1, 500).Select(i => (double)i).ToList();

        SetupProcessDataServiceMock(path, JsonConvert.SerializeObject(profileEntries));
    }

    private void MockMeanValue(ExtrusionThicknessMeasurementType profileType = ExtrusionThicknessMeasurementType.Primary, Exception? exception = null)
    {
        var path = profileType switch
        {
            ExtrusionThicknessMeasurementType.MdoWinderA => Constants.LastPartOfPath.MdoProfileAMeanValue,
            ExtrusionThicknessMeasurementType.MdoWinderB => Constants.LastPartOfPath.MdoProfileBMeanValue,
            _ => Constants.LastPartOfPath.PrimaryProfileMeanValue,
        };

        if (exception is not null)
        {
            SetupProcessDataServiceMock(path, exception: exception);
            return;
        }

        SetupProcessDataServiceMock(path, 100.0, _variableUnits);
    }

    private void MockTwoSigma(ExtrusionThicknessMeasurementType profileType = ExtrusionThicknessMeasurementType.Primary, Exception? exception = null)
    {
        var path = profileType switch
        {
            ExtrusionThicknessMeasurementType.MdoWinderA => Constants.LastPartOfPath.MdoProfileATwoSigma,
            ExtrusionThicknessMeasurementType.MdoWinderB => Constants.LastPartOfPath.MdoProfileBTwoSigma,
            _ => Constants.LastPartOfPath.PrimaryProfileTwoSigma,
        };

        if (exception is not null)
        {
            SetupProcessDataServiceMock(path, exception: exception);
            return;
        }

        SetupProcessDataServiceMock(path, 200.0, _variableUnits);
    }

    private void MockIsControllerOn(
        ExtrusionThicknessMeasurementType profileType = ExtrusionThicknessMeasurementType.Primary,
        bool state = true,
        Exception? exception = null)
    {
        var profileControlPath = Constants.LastPartOfPath.ProfileControl;

        var otherPath = profileType switch
        {
            ExtrusionThicknessMeasurementType.Primary => Constants.LastPartOfPath.ThicknessGauge,
            ExtrusionThicknessMeasurementType.MdoWinderA => Constants.LastPartOfPath.WinderAContactDrive,
            _ => Constants.LastPartOfPath.WinderBContactDrive
        };

        if (exception is not null)
        {
            SetupProcessDataServiceMock(profileControlPath, exception: exception);
            SetupProcessDataServiceMock(otherPath, exception: exception);
            return;
        }

        var profileControlValue = profileType switch
        {
            ExtrusionThicknessMeasurementType.Primary => 1.0,
            ExtrusionThicknessMeasurementType.MdoWinderA => 4.0,
            _ => 4.0
        };

        SetupProcessDataServiceMock(profileControlPath, profileControlValue);
        SetupProcessDataServiceMock(
            otherPath,
             profileType switch
             {
                 ExtrusionThicknessMeasurementType.Primary => state,
                 ExtrusionThicknessMeasurementType.MdoWinderA => state ? 1.0 : 0.0,
                 _ => state ? 1.0 : 0.0
             });
    }

    private void MockControlElements(Exception? exception = null)
    {
        var path = Constants.LastPartOfPath.ControlElements;

        if (exception is not null)
        {
            SetupProcessDataServiceMock(path, exception: exception);
            return;
        }

        var controlElements = new List<double>
        {
            1,
            2,
            3
        };

        SetupProcessDataServiceMock(path, JsonConvert.SerializeObject(controlElements));
    }

    private void SetupProcessDataServiceMock(string path, object? value = null, VariableUnits? unit = null, Exception? exception = null)
    {
        (ProcessData processData, ProcessVariableMetaDataResponseItem metaData)? dataResultValue =
            exception is not null
                ? null
                : new(
                    new ProcessData()
                    {
                        Value = value,
                        Timestamp = _timestamp,
                        Path = path
                    },
                    new ProcessVariableMetaDataResponseItem()
                    {
                        Data = new ProcessVariableMetaData()
                        {
                            Units = unit,
                            VariableIdentifier = path
                        }
                    }
                    );

        _processDataServiceMock
            .Setup(mock => mock.GetProcessDataByLastPartOfPath(
                It.IsAny<ProcessDataByTimestampBatchDataLoader>(),
                It.IsAny<MachineMetaDataBatchDataLoader>(),
                It.IsAny<LatestProcessDataCacheDataLoader>(),
                _machineId,
                path,
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<(ProcessData processData, ProcessVariableMetaDataResponseItem metaData)?>(dataResultValue, exception));
    }
}