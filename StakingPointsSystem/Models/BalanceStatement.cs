﻿using StakingPointsSystem.Services;

namespace StakingPointsSystem.Models;

public class BalanceStatement : IStatement
{
    public BalanceStatement(int userId, AssetType assetType, decimal unit)
    {
        UserId = userId;
        AssetType = assetType;
        Unit = unit;
    }

    public override int UserId { get; set; }
    protected override decimal Unit { get; set; }
    protected override AssetType AssetType { get; set; }

    protected override decimal CurrentScore(DateRange updatePeriod)
    {
        var totalScore = (int)(updatePeriod.EndTime.TrimMilliseconds() - updatePeriod.StartTime.TrimMilliseconds()).TotalSeconds * GetBaseScore(AssetType) *
                         Unit;
        return totalScore;
    }
}