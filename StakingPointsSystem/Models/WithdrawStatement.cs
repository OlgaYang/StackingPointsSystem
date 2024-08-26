namespace StakingPointsSystem.Models;

public class WithdrawStatement : IStatement
{
    public WithdrawStatement(int userId, DateTime createdTime, AssetType assetType, int unit)
    {
        UserId = userId;
        WithdrawTime = createdTime;
        AssetType = assetType;
        Unit = unit;
    }

    public override int UserId { get; set; }
    protected override decimal Unit { get; set; }
    protected override AssetType AssetType { get; set; }
    private DateTime WithdrawTime { get; set; }

    protected override decimal CurrentScore(DateRange updatePeriod)
    {
        // add score before withdraw
        var totalScore = (int)(WithdrawTime - updatePeriod.StartTime).TotalSeconds * GetBaseScore(AssetType) * Unit;
        return totalScore;
    }
}