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
        var users = _dbContext.Users
            .Join(_dbContext.Assets, user => user.UserId, asset => asset.UserId, (user, asset) => user).ToList();
        foreach (var user in users)
        {
            var balanceStatements = await
                _dbContext.Balances.Where(x => x.UserId == user.UserId).Select(x => x.ToStatement()).ToListAsync();
            var userScore = await _dbContext.UserScores.FirstOrDefaultAsync(x => x.UserId == user.UserId);
            var lastUpdatedTime = userScore?.LastUpdatedTime ?? await GetFistDepositTime(user.UserId);

            var assetStatements = await _dbContext.Assets.Where(x =>
                    x.UserId == user.UserId && x.CreatedTime < updatedTime && x.CreatedTime >= lastUpdatedTime)
                .Select(x => x.ToStatement()).ToListAsync();

            IStatement previousStatement = GetInitStatement(userScore, user.UserId);

            foreach (var statement in balanceStatements.Concat(assetStatements))
            {
                statement.SetPrevious(previousStatement);
                previousStatement = statement;
            }

            await AddOrUpdateScore(previousStatement, new DateRange()
            {
                StartTime = lastUpdatedTime,
                EndTime = updatedTime
            });
        }

        await _dbContext.SaveChangesAsync();
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