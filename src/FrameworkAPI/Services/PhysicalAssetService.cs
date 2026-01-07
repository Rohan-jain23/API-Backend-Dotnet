using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.PhysicalAsset;
using FrameworkAPI.Schema.PhysicalAsset.Defect;
using FrameworkAPI.Schema.PhysicalAsset.History;
using FrameworkAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using PhysicalAssetDataHandler.Client.HttpClients;
using PhysicalAssetDataHandler.Client.Models.Enums;
using PhysicalAssetDataHandler.Client.QueueWrappers;
using CreateAniloxPhysicalAssetRequest = FrameworkAPI.Schema.PhysicalAsset.CreateAniloxPhysicalAssetRequest;
using UpdateAniloxPhysicalAssetRequest = FrameworkAPI.Schema.PhysicalAsset.UpdateAniloxPhysicalAssetRequest;
using Messages = PhysicalAssetDataHandler.Client.Models.Messages;

namespace FrameworkAPI.Services;

public class PhysicalAssetService(
    IPhysicalAssetSettingsHttpClient physicalAssetSettingsHttpClient,
    IPhysicalAssetHttpClient physicalAssetHttpClient,
    IPhysicalAssetQueueWrapper physicalAssetQueueWrapper) : IPhysicalAssetService
{
    public async Task<PhysicalAssetSettings> GetPhysicalAssetSettings(
        CancellationToken cancellationToken)
    {
        var getPhysicalAssetSettingsResponse = await physicalAssetSettingsHttpClient.GetPhysicalAssetSettings(cancellationToken);

        if (getPhysicalAssetSettingsResponse.HasError)
        {
            throw new InternalServiceException(getPhysicalAssetSettingsResponse.Error);
        }

        return new PhysicalAssetSettings(getPhysicalAssetSettingsResponse.Item);
    }

    public async Task<IEnumerable<PhysicalAsset>> GetAllPhysicalAssets(
        PhysicalAssetsFilter physicalAssetsFilter,
        PhysicalAssetType? physicalAssetTypeFilter,
        DateTime? lastChangeFilter,
        CancellationToken cancellationToken)
    {
        var getPhysicalAssetsResponse = await physicalAssetHttpClient.GetPhysicalAssets(
            physicalAssetsFilter, physicalAssetTypeFilter, lastChangeFilter, cancellationToken);

        if (getPhysicalAssetsResponse.HasError &&
            getPhysicalAssetsResponse.Error.StatusCode != StatusCodes.Status204NoContent)
        {
            throw new InternalServiceException(getPhysicalAssetsResponse.Error);
        }

        return getPhysicalAssetsResponse.Items.Select(PhysicalAsset.CreateInstance);
    }

    public async Task<PhysicalAsset> GetPhysicalAsset(string physicalAssetId, CancellationToken cancellationToken)
    {
        var getPhysicalAssetResponse =
            await physicalAssetHttpClient.GetPhysicalAsset(physicalAssetId, cancellationToken);

        if (getPhysicalAssetResponse.HasError)
        {
            throw new InternalServiceException(getPhysicalAssetResponse.Error);
        }

        return PhysicalAsset.CreateInstance(getPhysicalAssetResponse.Item);
    }

    public async Task<IEnumerable<PhysicalAssetHistoryItem>?> GetHistory(
        PhysicalAssetHistoryBatchDataLoader physicalAssetHistoryBatchDataLoader,
        string physicalAssetId)
    {
        var (physicalAssetHistoryItems, exception) =
            await physicalAssetHistoryBatchDataLoader.LoadAsync(physicalAssetId);

        if (exception is not null)
        {
            throw exception;
        }

        return physicalAssetHistoryItems?.Select(PhysicalAssetHistoryItem.CreateInstance);
    }

    public async Task<IEnumerable<PhysicalAssetDefect>?> GetDefects(
        PhysicalAssetDefectsBatchDataLoader physicalAssetDefectsBatchDataLoader,
        string physicalAssetId)
    {
        var (physicalAssetDefects, exception) = await physicalAssetDefectsBatchDataLoader.LoadAsync(physicalAssetId);

        if (exception is not null)
        {
            throw exception;
        }

        return physicalAssetDefects?.Select(PhysicalAssetDefect.CreateInstance);
    }

    public async Task<PhysicalAssetSettings> UpdatePhysicalAssetSettings(
        UpdatePhysicalAssetSettingsRequest updatePhysicalAssetSettingsRequest, string userId)
    {
        var updatePhysicalAssetSettingsRequestMessage = new Messages.UpdatePhysicalAssetSettingsRequest(
            userId,
            updatePhysicalAssetSettingsRequest.AniloxCleaningIntervalInMeter);

        var response = await physicalAssetQueueWrapper
            .SendUpdatePhysicalAssetSettingsRequestAndWaitForReply(updatePhysicalAssetSettingsRequestMessage);

        if (!response.HasError)
        {
            return new PhysicalAssetSettings(response.Item);
        }

        if (response.Error.StatusCode is StatusCodes.Status400BadRequest or StatusCodes.Status409Conflict)
        {
            throw new ParameterInvalidException(response.Error.ErrorMessage);
        }

        throw new InternalServiceException(response.Error);
    }

    public async Task<AniloxPhysicalAsset> CreateAniloxPhysicalAsset(
        CreateAniloxPhysicalAssetRequest createAniloxPhysicalAssetRequest, string userId)
    {
        var createAniloxPhysicalAssetRequestMessage =
            new Messages.PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest(
                userId,
                createAniloxPhysicalAssetRequest.SerialNumber,
                createAniloxPhysicalAssetRequest.Manufacturer,
                createAniloxPhysicalAssetRequest.Description,
                createAniloxPhysicalAssetRequest.DeliveredAt,
                createAniloxPhysicalAssetRequest.PreferredUsageLocation,
                createAniloxPhysicalAssetRequest.InitialUsageCounter,
                createAniloxPhysicalAssetRequest.InitialTimeUsageCounter,
                createAniloxPhysicalAssetRequest.ScanCodes.Where(scanCode => !string.IsNullOrWhiteSpace(scanCode)),
                createAniloxPhysicalAssetRequest.PrintWidth,
                createAniloxPhysicalAssetRequest.IsSleeve,
                createAniloxPhysicalAssetRequest.InnerDiameter,
                createAniloxPhysicalAssetRequest.OuterDiameter,
                createAniloxPhysicalAssetRequest.Screen,
                createAniloxPhysicalAssetRequest.Engraving,
                createAniloxPhysicalAssetRequest.SetVolumeValue,
                createAniloxPhysicalAssetRequest.SetOpticalDensityValue,
                createAniloxPhysicalAssetRequest.MeasuredVolumeValue);

        var response = await physicalAssetQueueWrapper
            .SendCreateAniloxPhysicalAssetRequestAndWaitForReply(createAniloxPhysicalAssetRequestMessage);

        if (!response.HasError)
        {
            return new AniloxPhysicalAsset(response.Item);
        }

        if (response.Error.StatusCode is StatusCodes.Status400BadRequest or StatusCodes.Status409Conflict)
        {
            throw new ParameterInvalidException(response.Error.ErrorMessage);
        }

        throw new InternalServiceException(response.Error);
    }

    public async Task<AniloxPhysicalAsset> UpdateAniloxPhysicalAsset(
        UpdateAniloxPhysicalAssetRequest updateAniloxPhysicalAssetRequest, string userId)
    {
        var updateAniloxPhysicalAssetRequestMessage =
            new Messages.PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest(
                userId,
                updateAniloxPhysicalAssetRequest.PhysicalAssetId,
                updateAniloxPhysicalAssetRequest.SerialNumber,
                updateAniloxPhysicalAssetRequest.Manufacturer,
                updateAniloxPhysicalAssetRequest.Description,
                updateAniloxPhysicalAssetRequest.DeliveredAt,
                updateAniloxPhysicalAssetRequest.PreferredUsageLocation,
                updateAniloxPhysicalAssetRequest.InitialUsageCounter,
                updateAniloxPhysicalAssetRequest.InitialTimeUsageCounter,
                updateAniloxPhysicalAssetRequest.ScanCodes.Where(scanCode => !string.IsNullOrWhiteSpace(scanCode)),
                updateAniloxPhysicalAssetRequest.PrintWidth,
                updateAniloxPhysicalAssetRequest.IsSleeve,
                updateAniloxPhysicalAssetRequest.InnerDiameter,
                updateAniloxPhysicalAssetRequest.OuterDiameter,
                updateAniloxPhysicalAssetRequest.Screen,
                updateAniloxPhysicalAssetRequest.Engraving,
                updateAniloxPhysicalAssetRequest.SetVolumeValue,
                updateAniloxPhysicalAssetRequest.SetOpticalDensityValue);

        var response = await physicalAssetQueueWrapper
            .SendUpdateAniloxPhysicalAssetRequestAndWaitForReply(updateAniloxPhysicalAssetRequestMessage);

        if (!response.HasError)
        {
            return new AniloxPhysicalAsset(response.Item);
        }

        if (response.Error.StatusCode is StatusCodes.Status400BadRequest or StatusCodes.Status409Conflict)
        {
            throw new ParameterInvalidException(response.Error.ErrorMessage);
        }

        throw new InternalServiceException(response.Error);
    }
}