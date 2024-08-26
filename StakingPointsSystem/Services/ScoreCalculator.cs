using Microsoft.EntityFrameworkCore.Internal;
using StakingPointsSystem.Models;

namespace StakingPointsSystem.Services;

public class ScoreCalculator
{
    private readonly StakingPointsDbContext _dbContext;

    public ScoreCalculator(StakingPointsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Calculate(DateTime updatedTime)
    {
        var assetsWithScore = _dbContext.Assets
            .GroupJoin(
                _dbContext.UserScores,
                a => a.UserId,
                s => s.UserId,
                (a, s) => new { a, s }
            )
            .SelectMany(
                x => x.s.DefaultIfEmpty(),
                (x, s) => new { x.a, s }
            )
            .Where(x => x.a.CreatedTime <= updatedTime 
                        && x.a.CreatedTime > (x.s == null ? new DateTime(2024, 1, 1) : x.s.LastUpdatedTime))
            .Select(x => new 
            {
                Score = x.s,
                Asset = x.a
            })
            .ToList();
        
        if (!assetsWithScore.Any())
        {
            return;
        }
        
        foreach (var grouping in assetsWithScore.GroupBy(x=>x.Asset.UserId))
        {
            var userScore1 = grouping.First().Score;
            var score = userScore1 == null ? 0 : userScore1.TotalScore ;

            foreach (var assetWithScore in grouping)
            {
                var totalSeconds = (int)(updatedTime - assetWithScore.Asset.CreatedTime).TotalSeconds;
                var baseScore = GetBaseScore(assetWithScore.Asset.AssetType);
                var totalScore = baseScore * assetWithScore.Asset.Unit * totalSeconds;
                if (assetWithScore.Asset.TransactionType == TransactionType.Deposit)
                {
                    score += totalScore;
                }
                else if(assetWithScore.Asset.TransactionType == TransactionType.Withdraw)
                {
                    score -= totalScore;   
                }
            }

            //TODO: 補上init score
            var userScore = _dbContext.UserScores.Find(grouping.Key);
            if (userScore == null)
            {
                _dbContext.Add(new UserScore
                {
                    UserId = grouping.Key,
                    TotalScore = score,
                    LastUpdatedTime = updatedTime
                });
            }
            else
            {
                userScore!.TotalScore = score;
                userScore.LastUpdatedTime = updatedTime; 
            }
        }
       
        await _dbContext.SaveChangesAsync();
       
    }

    private int GetBaseScore(AssetType assetType)
    {
        switch(assetType)
        {
            case AssetType.Apple:
                return 100;
            case AssetType.Banana:
                return 20;
            case AssetType.Kiwi:
                return 10;
            default:
                throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
        }
    }
}