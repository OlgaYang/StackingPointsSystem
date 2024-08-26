namespace StakingPointsSystem.Models;

public class BalanceStatement : IStatement
{
    public BalanceStatement(int userId, AssetType assetType, int unit)
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
        var totalScore = (int)(updatePeriod.EndTime - updatePeriod.StartTime).TotalSeconds * GetBaseScore(AssetType) *
                         Unit;
        return totalScore;
    }
}