using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StakingPointsSystem;
using StakingPointsSystem.Models;
using StakingPointsSystem.Services;

namespace StakingPointsSystemTests;

public class AssetManagerTests
{
    private DbContextOptions<StakingPointsDbContext> _options;
    private StakingPointsDbContext _mockContext;
    private AssetManager _assetManager;

    [SetUp]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<StakingPointsDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _mockContext = new StakingPointsDbContext(_options);
        _assetManager = new AssetManager(_mockContext);
    }

    [Test]
    public async Task user_deposit_a_banana()
    {
        var asset = new
        {
            UserId = 1,
            Unit = 4,
            AssetType = AssetType.Banana,
            TransactionType = TransactionType.Deposit
        };

        await _assetManager.Deposit(asset.UserId, asset.Unit, asset.AssetType);

        _mockContext.Assets.Single(x => x.UserId == asset.UserId && x.AssetType == asset.AssetType).Should()
            .BeEquivalentTo(asset);
    }

    [Test]
    public void METHOD()
    {
        var s = new DateTime(2024, 08, 26, 21, 18, 31, 893);
        var e = new DateTime(2024, 08, 26, 21, 05, 00);
        var totalSeconds = (s - e).TotalSeconds;
        //323200.00
    }
}