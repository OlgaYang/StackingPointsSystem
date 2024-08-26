using StakingPointsSystem.Models;
using StakingPointsSystem.Services;

namespace StakingPointsSystem.Interfaces;

public interface IAssetManager
{
    Task Deposit(int userId, int unit, AssetType assetType);
}