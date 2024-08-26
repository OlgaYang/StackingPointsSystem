using StakingPointsSystem.Services;

namespace StakingPointsSystem.Models;

public class Asset
{
    public int UserId { get; set; }
    public TransactionType TransactionType { get; set; }
    public AssetType AssetType { get; set; }
    public decimal Unit { get; set; }
    public DateTime CreatedTime { get; set; }

    public IStatement ToStatement()
    {
        if (TransactionType == TransactionType.Deposit)
        {
            return new DepositStatement(UserId,CreatedTime,AssetType,Unit);
        }
        
        if (TransactionType == TransactionType.Withdraw)
        {
            return new WithdrawStatement(UserId,CreatedTime,AssetType,Unit); 
        }
        
        throw new ArgumentException("Invalid transaction type");
    }
}