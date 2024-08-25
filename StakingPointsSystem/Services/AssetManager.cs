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

    public async Task Deposit(Asset asset)
    {
        _stakingPointsDbContext.Assets.Add(asset);
        await _stakingPointsDbContext.SaveChangesAsync();
    }
}