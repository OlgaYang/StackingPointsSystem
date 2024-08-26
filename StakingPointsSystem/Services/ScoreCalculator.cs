﻿using System.Runtime.CompilerServices;
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


        foreach (var grouping in assetsWithScore.GroupBy(x => x.Asset.UserId))
        {
            var initScore = grouping.First().Score;
            var userId = grouping.Key;
            var initStatement = initScore == null
                ? new InitStatement(0, userId)
                : new InitStatement(initScore.TotalScore, userId);

            var firstDeposit = _dbContext.Assets.OrderBy(x => x.CreatedTime)
                .First(x => x.UserId == userId && x.TransactionType == TransactionType.Deposit);

            var updatePeriod = new DateRange()
            {
                StartTime = initScore?.LastUpdatedTime ?? firstDeposit.CreatedTime,
                EndTime = updatedTime
            };

            IStatement previousStatement = initStatement;
            var statements = _dbContext.Balances.Where(x => x.UserId == userId).Select(x => x.ToStatement()).ToList()
                .Concat(grouping.Select(x => x.Asset.ToStatement()));
            foreach (var statement in statements)
            {
                statement.SetPrevious(previousStatement);
                previousStatement = statement;
            }

            await SaveScore(previousStatement, updatePeriod);
        }


        await _dbContext.SaveChangesAsync();
    }

    private async Task SaveScore(IStatement previousStatement, DateRange updatePeriod)
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