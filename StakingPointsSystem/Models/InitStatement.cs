namespace StakingPointsSystem.Models;

public class InitStatement : IStatement
{
    public override int UserId { get; set; }
    protected override decimal Unit { get; set; }
    protected override AssetType AssetType { get; set; }
    private readonly decimal _initScore;

    public InitStatement(decimal initScore, int userId)
    {
        _initScore = initScore;
        UserId = userId;
    }

    public override decimal TotalScore(DateRange updatePeriod)
    {
        return CurrentScore(updatePeriod);
    }

    protected override decimal CurrentScore(DateRange updatePeriod)
    {
        return _initScore;
    }
}