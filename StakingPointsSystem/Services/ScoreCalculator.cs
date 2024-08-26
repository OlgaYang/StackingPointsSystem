using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
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
        var assetsWithScore = await GetScoreWithAssets(updatedTime);
        
        foreach (var grouping in assetsWithScore.GroupBy(x => x.Asset.UserId))
        {
            var userId = grouping.Key;

            IStatement previousStatement = GetInitStatement(grouping.First().Score, userId);
            
            var statements = _dbContext.Balances.Where(x => x.UserId == userId).Select(x => x.ToStatement()).ToList()
                .Concat(grouping.Select(x => x.Asset.ToStatement()));
            
            foreach (var statement in statements)
            {
                statement.SetPrevious(previousStatement);
                previousStatement = statement;
            }

            var updatePeriod = new DateRange()
            {
                StartTime = await GetStartTime(grouping, userId),
                EndTime = updatedTime
            };
            await AddOrUpdateScore(previousStatement, updatePeriod);
        }
        
        await _dbContext.SaveChangesAsync();
    }

    private async Task<DateTime> GetStartTime(IGrouping<int, ScoreWithAsset> grouping, int userId)
    {
        return grouping.First().Score?.LastUpdatedTime ?? await GetFistDepositTime(userId);
    }

    private static InitStatement GetInitStatement(UserScore? initScore, int userId)
    {
        return initScore == null
            ? new InitStatement(0, userId)
            : new InitStatement(initScore.TotalScore, userId);
    }

    private async Task<DateTime> GetFistDepositTime(int userId)
    {
        return (await _dbContext.Assets.OrderBy(x => x.CreatedTime)
            .FirstAsync(x => x.UserId == userId && x.TransactionType == TransactionType.Deposit)).CreatedTime;
    }

    private async Task<List<ScoreWithAsset>> GetScoreWithAssets(DateTime updatedTime)
    {
        return await _dbContext.Assets
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
                        && x.a.CreatedTime > (x.s == null ? System.Data.SqlTypes.SqlDateTime.MinValue.Value : x.s.LastUpdatedTime))
            .Select(x => new ScoreWithAsset()
            {
                Score = x.s,
                Asset = x.a
            })
            .ToListAsync();
    }

    private async Task AddOrUpdateScore(IStatement previousStatement, DateRange updatePeriod)
    {
        var userScore = await _dbContext.UserScores.FindAsync(previousStatement.UserId);
        if (userScore == null)
        {
            _dbContext.Add(new UserScore
            {
                UserId = previousStatement.UserId,
                TotalScore = previousStatement.TotalScore(updatePeriod),
                LastUpdatedTime = updatePeriod.EndTime
            });
        }
        else
        {
            userScore!.TotalScore = previousStatement.TotalScore(updatePeriod);
            userScore.LastUpdatedTime = updatePeriod.EndTime;
        }
    }
}

public class ScoreWithAsset
{
    public UserScore? Score { get; set; }
    public Asset Asset { get; set; }
}