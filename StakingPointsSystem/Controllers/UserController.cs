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
            if (Enum.TryParse(asset.Key, true, out AssetType assetType))
            {
                await _assetManager.Deposit(userId, asset.Value, assetType);
            }
            else
            {
                throw new ArgumentException("Invalid asset type");
            }
        }
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


    [HttpGet]
    public async Task<List<User>> Test()
    {
        var random = new Random();
        for (int i = 1000; i < 1000000; i++)
        {
            var asset = new Asset
            {
                UserId = i,
                TransactionType = TransactionType.Deposit,
                Unit = random.Next(0, 100),
                CreatedTime = DateTime.Now,
                AssetType = (AssetType)random.Next(1, 4),
            };

            _dbContext.Assets.Add(asset);
        }

        await _dbContext.SaveChangesAsync();
        return _dbContext.Users.Select(x => x).ToList();
    }
}

public class AssetRequest
{
    public decimal Score { get; set; }
    public int Rank { get; set; }
    public Dictionary<string, decimal> Balances { get; set; }
}