using StakingPointsSystem.Services;

namespace StakingPointsSystem.Models;

public class Asset
{
    public int UserId { get; set; }
    public TransactionType TransactionType { get; set; }
    public AssetType AssetType { get; set; }
    public int Unit { get; set; }
    public DateTime CreatedTime { get; set; }
}