using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StakingPointsSystem.Interfaces;
using StakingPointsSystem.Models;

namespace StakingPointsSystem.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly StakingPointsDbContext _dbContext;
    private readonly IAssetManager _assetManager;

    public UserController(StakingPointsDbContext dbContext, IAssetManager assetManager)
    {
        _dbContext = dbContext;
        _assetManager = assetManager;
    }

    [HttpGet]
    public async Task<AssetRequest> Assets(int userId)
    {
        var userScore = _dbContext.UserScores.FirstOrDefault(x => x.UserId == userId);
        var score = userScore?.TotalScore ?? 0;

        return new AssetRequest
        {
            Score = score,
            Rank = _dbContext.UserScores.Count(x => x.TotalScore > score) + 1,
            Balances = await _dbContext.Balances.Where(x => x.UserId == userId)
                .ToDictionaryAsync(x => x.AssetType.ToString(), x => x.Unit)
        };
    }

    [HttpPost]
    public async Task Deposit(int userId, Dictionary<string, decimal> assets)
    {
        foreach (var asset in assets)
        {
            if (!Enum.TryParse(asset.Key, true, out AssetType assetType))
            {
                throw new ArgumentException("Invalid asset type");
            }
        }
        
        await _assetManager.Deposit(userId, assets);
    }

    [HttpPost]
    public async Task Withdraw(int userId, Dictionary<string, decimal> assets)
    {
        foreach (var asset in assets)
        {
            if (!Enum.TryParse(asset.Key, true, out AssetType assetType))
            {
                throw new ArgumentException("Invalid asset type");
            }
        }

        await _assetManager.Withdraw(userId, assets);
    }
}