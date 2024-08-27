using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StakingPointsSystem.Controllers;
using StakingPointsSystem.Interfaces;
using StakingPointsSystem.Models;

namespace StakingPointsSystem.Services;

public class AssetManager : IAssetManager
{
    private readonly StakingPointsDbContext _stakingPointsDbContext;

    public AssetManager(StakingPointsDbContext stakingPointsDbContext)
    {
        _stakingPointsDbContext = stakingPointsDbContext;
    }
    
    public async Task Withdraw(int userId, Dictionary<string, decimal> assets)
    {
        using var transaction = await _stakingPointsDbContext.Database.BeginTransactionAsync();
        try
        {
            foreach (var asset in assets)
            {
                await Withdraw(userId, asset.Value, Enum.Parse<AssetType>(asset.Key, true));
            }

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task Deposit(int userId, Dictionary<string, decimal> assets)
    {
        using var transaction = await _stakingPointsDbContext.Database.BeginTransactionAsync();
        try
        {
            foreach (var asset in assets)
            {
                await Deposit(userId, asset.Value, Enum.Parse<AssetType>(asset.Key, true));
            }

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task Deposit(int userId, decimal unit, AssetType assetType)
    {
        await DepositBalance(userId, unit, assetType);
        await InsertAsset(userId, unit, assetType, TransactionType.Deposit);
    }

    private async Task Withdraw(int userId, decimal unit, AssetType assetType)
    {
        await WithdrawBalance(userId, unit, assetType);
        await InsertAsset(userId, unit, assetType, TransactionType.Withdraw);
    }

    private async Task WithdrawBalance(int userId, decimal unit, AssetType assetType)
    {
        var sql =
            @"IF EXISTS (SELECT 1 FROM Balances WITH(XLOCK, ROWLOCK) WHERE UserId = @UserId and AssetType = @AssetType and Unit >= @Unit )
BEGIN
   UPDATE Balances 
   SET Unit = Unit - @Unit
   WHERE UserId = @UserId and AssetType = @AssetType
END";
        var affectedRow = await _stakingPointsDbContext.Database.ExecuteSqlRawAsync(sql,
            new SqlParameter("@Unit", unit),
            new SqlParameter("UserId", userId), new SqlParameter("@AssetType", assetType));
        if (affectedRow == -1)
        {
            throw new WithdrawFailException("Not enough balance");
        }
    }

    private async Task InsertAsset(int userId, decimal unit, AssetType assetType, TransactionType transactionType)
    {
        var asset = new Asset()
        {
            AssetType = assetType,
            Unit = unit,
            UserId = userId,
            TransactionType = transactionType,
            CreatedTime = DateTime.Now
        };

        _stakingPointsDbContext.Assets.Add(asset);
        await _stakingPointsDbContext.SaveChangesAsync();
    }

    private async Task DepositBalance(int userId, decimal unit, AssetType assetType)
    {
        string sql =
            @" IF EXISTS (SELECT 1 FROM Balances WITH(XLOCK, ROWLOCK) WHERE UserId = @UserId and AssetType = @AssetType)
BEGIN
   UPDATE Balances 
   SET Unit = Unit + @Unit
   WHERE UserId = @UserId and AssetType = @AssetType

END
ELSE
BEGIN   
    INSERT INTO Balances (UserId, AssetType, Unit)
    VALUES (@UserId, @AssetType,  @Unit);
END";

        await _stakingPointsDbContext.Database.ExecuteSqlRawAsync(sql, new SqlParameter("@Unit", unit),
            new SqlParameter("UserId", userId), new SqlParameter("@AssetType", assetType));
    }
}

public class WithdrawFailException : Exception
{
    public WithdrawFailException(string empty)
    {
        throw new Exception(empty);
    }
}