namespace StakingPointsSystem.Models;

public abstract class IStatement
{
    private IStatement _previousStatement;
    public abstract int UserId { get; set; }
    protected abstract decimal Unit { get; set; }
    protected abstract AssetType AssetType { get; set; }

    public void SetPrevious(IStatement previousStatement)
    {
        _previousStatement = previousStatement;
    }

    public virtual decimal TotalScore(DateRange updatePeriod)
    {
        return _previousStatement.TotalScore(updatePeriod) + CurrentScore(updatePeriod);
    }
    protected abstract decimal CurrentScore(DateRange updatePeriod);

    protected int GetBaseScore(AssetType assetType)
    {
        switch (assetType)
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