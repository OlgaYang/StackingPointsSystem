namespace StakingPointsSystem.Models;

public class AssetRequest
{
    public decimal Score { get; set; }
    public int Rank { get; set; }
    public Dictionary<string, decimal> Balances { get; set; }
}