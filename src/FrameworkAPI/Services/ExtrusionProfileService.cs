using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using FrameworkAPI.Schema.Machine.ActualProcessValues;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using Newtonsoft.Json.Linq;
using WuH.Ruby.MetaDataHandler.Client;

namespace FrameworkAPI.Services;

public class ExtrusionProfileService(IProcessDataService processDataService) : IExtrusionProfileService
{
    public async Task<ExtrusionThicknessProfile?> GetMostRelevantProfile(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        string machineId,
        MachineFamily machineFamily,
        DateTime? timestamp,
        CancellationToken cancellationToken
    )
    {
        Task<ExtrusionThicknessProfile?> PrimaryProfile() => GetProfile(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            machineId,
            machineFamily,
            timestamp,
            cancellationToken);

        Task<ExtrusionThicknessProfile?> MdoProfileA() => GetProfile(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.MdoWinderA,
            machineId,
            machineFamily,
            timestamp,
            cancellationToken);

        Task<ExtrusionThicknessProfile?> MdoProfileB() => GetProfile(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.MdoWinderB,
            machineId,
            machineFamily,
            timestamp,
            cancellationToken);

        var (profileControlObject, _, _) = await GetProcessDataServiceResponse(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            machineId,
            Constants.LastPartOfPath.ProfileControl,
            timestamp,
            cancellationToken);

        // CastFilm machines provide true/false as string instead of double values in 'PRg1OnOf' so we have to handle that case separately
        var isCastFilm = profileControlObject is string profileControlString && bool.TryParse(profileControlString, out _);
        int? profileControl = 0;

        if (!isCastFilm)
        {
            profileControl = ParseProcessDataServiceItemResponse<int>(profileControlObject);
        }

        if (isCastFilm || profileControl is null || profileControl.Value != 4)
        {
            return await PrimaryProfile();
        }

        var (winderAContactDriveObject, _, _) = await GetProcessDataServiceResponse(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            machineId,
            Constants.LastPartOfPath.WinderAContactDrive,
            timestamp,
            cancellationToken);

        var winderAContactDrive = ParseProcessDataServiceItemResponse<int>(winderAContactDriveObject);

        if (winderAContactDrive is null)
        {
            return await PrimaryProfile();
        }

        return winderAContactDrive > 0
            ? await MdoProfileA()
            : await MdoProfileB();
    }

    public async Task<ExtrusionThicknessProfile?> GetProfile(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        ExtrusionThicknessMeasurementType profileType,
        string machineId,
        MachineFamily machineFamily,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        var (dataPoints, dataPointsTimestamp) = await GetDataPoints(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            profileType,
            machineId,
            timestamp,
            cancellationToken);

        var (meanValue, meanValueUnit) = await GetMeanValueAndUnit(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            profileType,
            machineId,
            timestamp,
            cancellationToken);

        var (twoSigmaValue, twoSigmaUnit) = await GetTwoSigmaValue(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            profileType,
            machineId,
            timestamp,
            cancellationToken);

        var isControllerOn = await IsControllerOn(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            profileType,
            machineId,
            machineFamily,
            timestamp,
            cancellationToken);

        var controlElements = await GetControlElements(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            profileType,
            machineId,
            timestamp,
            cancellationToken);

        IList<ProfileEntry>? profileData = null;

        if (meanValue is not null && dataPoints is not null)
        {
            profileData = Downsample(dataPoints, 480)
                .Select((dataPoint, index) =>
                    new ProfileEntry { Name = index.ToString(), Value = (1 - dataPoint / meanValue.Value) * -100 })
                .ToList();
        }

        var xAxisUnit = machineFamily == MachineFamily.BlowFilm ? "°" : "";

        return new ExtrusionThicknessProfile(
            profileType,
            profileData?.ToDictionary(entry => int.Parse(entry.Name), entry => entry.Value) ?? [],
            xAxisUnit,
            new NumericValue(
                valueFunc: _ => Task.FromResult(meanValue),
                unitFunc: _ => Task.FromResult(meanValueUnit)),
            new NumericValue(
                valueFunc: _ => Task.FromResult(twoSigmaValue),
                unitFunc: _ => Task.FromResult(twoSigmaUnit)),
            isControllerOn,
            controlElements,
            dataPointsTimestamp);
    }

    private async Task<(double[]? DataPoints, DateTime? DataPointsTimestamp)> GetDataPoints(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        ExtrusionThicknessMeasurementType profileType,
        string machineId,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        var (dataPointsObject, dataPointsTimestamp, _) = await GetProcessDataServiceResponse(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            machineId,
            GetDataPointsPath(profileType),
            timestamp,
            cancellationToken);

        var dataPoints = ParseProcessDataServiceListResponse<double>(dataPointsObject);

        return (dataPoints, dataPointsTimestamp);
    }

    private async Task<(double?, string?)> GetMeanValueAndUnit(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        ExtrusionThicknessMeasurementType profileType,
        string machineId,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        var (meanValueObject, _, metaDataResponse) = await GetProcessDataServiceResponse(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            machineId,
            GetMeanValuePath(profileType),
            timestamp,
            cancellationToken);

        var meanValue = ParseProcessDataServiceItemResponse<double>(meanValueObject);
        var meanValueUnit = metaDataResponse.Units?.Si.Unit;

        return (meanValue, meanValueUnit);
    }

    private async Task<(double?, string?)> GetTwoSigmaValue(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        ExtrusionThicknessMeasurementType profileType,
        string machineId,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        var (twoSigmaValueObject, _, metaDataResponse) = await GetProcessDataServiceResponse(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            machineId,
            GetTwoSigmaPath(profileType),
            timestamp,
            cancellationToken);

        var twoSigmaValue = ParseProcessDataServiceItemResponse<double>(twoSigmaValueObject);
        var twoSigmaUnit = metaDataResponse.Units?.Si.Unit;

        return (twoSigmaValue, twoSigmaUnit);
    }

    private async Task<bool?> IsControllerOn(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        ExtrusionThicknessMeasurementType profileType,
        string machineId,
        MachineFamily machineFamily,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        var (profileControlObject, _, _) = await GetProcessDataServiceResponse(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            machineId,
            Constants.LastPartOfPath.ProfileControl,
            timestamp,
            cancellationToken);

        // CastFilm machines provide true/false as string instead of double values in 'PRg1OnOf' so we have to handle that case separately
        int? profileControl = 0;
        bool? profileControlBool = false;
        var isCastFilm = machineFamily == MachineFamily.CastFilm;

        if (isCastFilm)
        {
            // Short circuit for castFilm machines that are not being requested for primary profile
            if (profileType != ExtrusionThicknessMeasurementType.Primary)
            {
                return false;
            }

            profileControlBool = ParseProcessDataServiceItemResponse<bool>(profileControlObject);

            if (profileControlBool is null)
            {
                return null;
            }
        }
        else
        {
            profileControl = ParseProcessDataServiceItemResponse<int>(profileControlObject);

            if (profileControl is null)
            {
                return null;
            }
        }

        switch (profileType)
        {
            case ExtrusionThicknessMeasurementType.Primary:
                {
                    var (thicknessGaugeObject, _, _) = await GetProcessDataServiceResponse(
                        processDataByTimestampBatchDataLoader,
                        machineMetaDataBatchDataLoader,
                        latestProcessDataCacheDataLoader,
                        machineId,
                        Constants.LastPartOfPath.ThicknessGauge,
                        timestamp,
                        cancellationToken);

                    var thicknessGauge = ParseProcessDataServiceItemResponse<bool>(thicknessGaugeObject);

                    if (thicknessGauge is null)
                    {
                        return null;
                    }

                    return
                        (new List<int> { 1, 2, 3 }.Contains(profileControl.Value) || (isCastFilm && profileControlBool.Value))
                        && thicknessGauge.Value;
                }
            case ExtrusionThicknessMeasurementType.MdoWinderA:
                {
                    var (winderAContactDriveObject, _, _) = await GetProcessDataServiceResponse(
                        processDataByTimestampBatchDataLoader,
                        machineMetaDataBatchDataLoader,
                        latestProcessDataCacheDataLoader,
                        machineId,
                        Constants.LastPartOfPath.WinderAContactDrive,
                        timestamp,
                        cancellationToken);

                    var winderAContactDrive = ParseProcessDataServiceItemResponse<int>(winderAContactDriveObject);

                    if (winderAContactDrive is null)
                    {
                        return null;
                    }

                    return profileControl.Value == 4 && winderAContactDrive.Value > 0;
                }
            case ExtrusionThicknessMeasurementType.MdoWinderB:
                {
                    var (winderBContactDriveObject, _, _) = await GetProcessDataServiceResponse(
                            processDataByTimestampBatchDataLoader,
                            machineMetaDataBatchDataLoader,
                            latestProcessDataCacheDataLoader,
                            machineId,
                            Constants.LastPartOfPath.WinderBContactDrive,
                            timestamp,
                            cancellationToken);

                    var winderBContactDrive = ParseProcessDataServiceItemResponse<int>(winderBContactDriveObject);

                    if (winderBContactDrive is null)
                    {
                        return null;
                    }

                    return profileControl.Value == 4 && winderBContactDrive.Value > 0;
                }
            default:
                throw new ArgumentException($"Unsupported profile type {profileType}.", nameof(profileType));
        }
    }

    private async Task<IDictionary<int, double>?> GetControlElements(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        ExtrusionThicknessMeasurementType profileType,
        string machineId,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        if (profileType != ExtrusionThicknessMeasurementType.Primary)
        {
            return null;
        }

        var (controlElementsObject, _, _) = await GetProcessDataServiceResponse(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            machineId,
            Constants.LastPartOfPath.ControlElements,
            timestamp,
            cancellationToken);

        var controlElements = ParseProcessDataServiceListResponse<double>(controlElementsObject);

        return controlElements?
            .Select((value, i) => (value, i))
            .ToDictionary(entry => entry.i, entry => entry.value);
    }

    private static string GetDataPointsPath(ExtrusionThicknessMeasurementType profileType)
        => profileType switch
        {
            ExtrusionThicknessMeasurementType.MdoWinderA => Constants.LastPartOfPath.MdoProfileA,
            ExtrusionThicknessMeasurementType.MdoWinderB => Constants.LastPartOfPath.MdoProfileB,
            _ => Constants.LastPartOfPath.PrimaryProfile,
        };

    private static string GetMeanValuePath(ExtrusionThicknessMeasurementType profileType)
        => profileType switch
        {
            ExtrusionThicknessMeasurementType.MdoWinderA => Constants.LastPartOfPath.MdoProfileAMeanValue,
            ExtrusionThicknessMeasurementType.MdoWinderB => Constants.LastPartOfPath.MdoProfileBMeanValue,
            _ => Constants.LastPartOfPath.PrimaryProfileMeanValue,
        };

    private static string GetTwoSigmaPath(ExtrusionThicknessMeasurementType profileType)
        => profileType switch
        {
            ExtrusionThicknessMeasurementType.MdoWinderA => Constants.LastPartOfPath.MdoProfileATwoSigma,
            ExtrusionThicknessMeasurementType.MdoWinderB => Constants.LastPartOfPath.MdoProfileBTwoSigma,
            _ => Constants.LastPartOfPath.PrimaryProfileTwoSigma,
        };

    private async Task<(object? Value, DateTime? Timestamp, ProcessVariableMetaData MetaData)> GetProcessDataServiceResponse(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        string machineId,
        string lastPartOfPath,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        var processDataServiceResponse = await processDataService.GetProcessDataByLastPartOfPath(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            machineId,
            lastPartOfPath,
            timestamp,
            cancellationToken);

        if (processDataServiceResponse.Exception is not null)
        {
            throw processDataServiceResponse.Exception;
        }

        var (processData, metaData) = processDataServiceResponse.Value!.Value;

        var processDataTimestamp = processData.Timestamp;

        return (processData.Value, processDataTimestamp, metaData.Data);
    }

    private static T? ParseProcessDataServiceItemResponse<T>(object? processDataServiceResponse) where T : struct, IParsable<T>
    {
        if (processDataServiceResponse is null)
        {
            return null;
        }

        if (T.TryParse(processDataServiceResponse.ToString()!, null, out var value))
        {
            return value;
        }

        return null;
    }

    private static T[]? ParseProcessDataServiceListResponse<T>(object? processDataServiceResponse) where T : IParsable<T>
    {
        if (processDataServiceResponse is null)
        {
            return null;
        }

        T[]? value;

        if (processDataServiceResponse is string stringResponse)
        {
            value = JArray
                .Parse(stringResponse)
                .ToObject<T[]>();
        }
        else
        {
            value = JArray.FromObject(processDataServiceResponse).ToObject<T[]>();
        }

        return value;
    }

    private static IEnumerable<double> Downsample(double[] source, int n)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var m = source.Length;
        if (m < n)
        {
            return source;
        }

        var destination = new double[n];

        for (var i = 0; i < n; i++)
        {
            var id = i + 0.5;
            var nearest = (int)(id / n * m);
            destination[i] = source[nearest];
        }

        return destination;
    }
}