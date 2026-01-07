using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.PhysicalAsset;
using FrameworkAPI.Schema.PhysicalAsset.Defect;
using FrameworkAPI.Schema.PhysicalAsset.History;
using PhysicalAssetDataHandler.Client.Models.Enums;
using UpdatePhysicalAssetSettingsRequest = FrameworkAPI.Schema.PhysicalAsset.UpdatePhysicalAssetSettingsRequest;

namespace FrameworkAPI.Services.Interfaces;

public interface IPhysicalAssetService
{
    Task<PhysicalAssetSettings> GetPhysicalAssetSettings(CancellationToken cancellationToken);

    Task<IEnumerable<PhysicalAsset>> GetAllPhysicalAssets(
        PhysicalAssetsFilter physicalAssetsFilter,
        PhysicalAssetType? physicalAssetTypeFilter,
        DateTime? lastChangeFilter,
        CancellationToken cancellationToken);

    Task<PhysicalAsset> GetPhysicalAsset(string physicalAssetId, CancellationToken cancellationToken);

    Task<IEnumerable<PhysicalAssetHistoryItem>?> GetHistory(
        PhysicalAssetHistoryBatchDataLoader physicalAssetHistoryBatchDataLoader,
        string physicalAssetId);

    Task<IEnumerable<PhysicalAssetDefect>?> GetDefects(
        PhysicalAssetDefectsBatchDataLoader physicalAssetDefectsBatchDataLoader,
        string physicalAssetId);

    Task<PhysicalAssetSettings> UpdatePhysicalAssetSettings(
        UpdatePhysicalAssetSettingsRequest updatePhysicalAssetSettingsRequest, string userId);

    Task<AniloxPhysicalAsset> CreateAniloxPhysicalAsset(
        CreateAniloxPhysicalAssetRequest createAniloxPhysicalAssetRequest, string userId);

    Task<AniloxPhysicalAsset> UpdateAniloxPhysicalAsset(
        UpdateAniloxPhysicalAssetRequest updateAniloxPhysicalAssetRequest, string userId);
}