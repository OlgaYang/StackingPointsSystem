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

    public async Task Deposit(int userId, int unit, AssetType assetType)
    {
        var asset = new Asset()
        {
            AssetType = assetType,
            Unit = unit,
            UserId = userId,
            TransactionType = TransactionType.Deposit,
            CreatedTime = DateTime.Now
        };

        _stakingPointsDbContext.Assets.Add(asset);
        await _stakingPointsDbContext.SaveChangesAsync();
    }

    public async Task Withdraw(string username, int unit, AssetType assetType)
    {
        // TODO: 檢查庫存
        // TODO: Avoid double click
        
        using (var transaction = await _stakingPointsDbContext.Database.BeginTransactionAsync())
        {
            var sql = "SELECT * FROM Assets WITH (XLOCK, ROWLOCK) WHERE Username = @Username";
            var id = 1;

            var entity = _stakingPointsDbContext.Assets
                .FromSqlRaw(sql, new SqlParameter("@Username", username))
                .FirstOrDefault();

            // 在這裡可以進行其他需要在同一個 XLOCK 上執行的操作

            transaction.Commit();
        }
        
        throw new NotImplementedException();
    }
}
