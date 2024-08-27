﻿using Microsoft.AspNetCore.Mvc;
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


    [HttpGet]
    public async Task<List<User>> Test()
    {
        var random = new Random();
        for (int i = 1; i < 10000; i++)
        {
            await _assetManager.Deposit(i, new Dictionary<string, decimal>()
            {
                { ((AssetType)random.Next(1, 3)).ToString(), random.Next(0, 20) }
            });
            // var asset = new Asset
            // {
            //     UserId = i,
            //     TransactionType = TransactionType.Deposit,
            //     Unit = random.Next(0, 20),
            //     CreatedTime = DateTime.Now,
            //     AssetType = (AssetType)random.Next(1, 3),
            // };
            //
            // _dbContext.Assets.Add(asset);
        }

        // await _dbContext.SaveChangesAsync();
        // return _dbContext.Users.Select(x => x).ToList();
        return null;
    }
}

public class AssetRequest
{
    public decimal Score { get; set; }
    public int Rank { get; set; }
    public Dictionary<string, decimal> Balances { get; set; }
}