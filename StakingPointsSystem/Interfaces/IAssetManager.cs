using StakingPointsSystem.Models;
using StakingPointsSystem.Services;

namespace StakingPointsSystem.Interfaces;

public interface IAssetManager
{
    Task Deposit(int userId, decimal unit, AssetType assetType);
    Task Withdraw(int userId, Dictionary<string,decimal> assets);
}