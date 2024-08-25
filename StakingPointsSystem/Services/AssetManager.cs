using StakingPointsSystem.Controllers;
using StakingPointsSystem.Interfaces;
using StakingPointsSystem.Models;

namespace StakingPointsSystem.Services;

public class AssetManager : IAssetManager
{
    private readonly StakingPointsDbContext _stakingPointsDbContext;

    public AssetManager(StakingPointsDbContext stakingPointsDbContext)
    {
        _stakingPointsDbContext = stakingPointsDbContext;
    }

    public async Task Deposit(string assetUsername, int assetUnit, AssetType assetAssetType)
    {
        var asset = new Asset()
        {
            AssetType = assetAssetType,
            Unit = assetUnit,
            Username = assetUsername,
            TransactionType = TransactionType.Deposit,
            CreatedTime = DateTime.Now
        };

        _stakingPointsDbContext.Assets.Add(asset);
        await _stakingPointsDbContext.SaveChangesAsync();
    }
}