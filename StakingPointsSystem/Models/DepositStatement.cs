namespace StakingPointsSystem.Models;

public class DepositStatement : IStatement
{
    public DepositStatement(int userId, DateTime createdTime, AssetType assetType, decimal unit)
    {
        UserId = userId;
        DepositTime = createdTime;
        AssetType = assetType;
        Unit = unit;
    }

    public override int UserId { get; set; }
    protected override decimal Unit { get; set; }
    protected override AssetType AssetType { get; set; }

    private DateTime DepositTime { get; set; }

    protected override decimal CurrentScore(DateRange updatePeriod)
    {
        // remove score before deposit
        var totalScore = ((int)(DepositTime - updatePeriod.StartTime).TotalSeconds) * GetBaseScore(AssetType) * Unit;
        return -totalScore;
    }
}