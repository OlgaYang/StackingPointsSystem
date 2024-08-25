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
            Username = "olga",
            Unit = 4,
            AssetType = AssetType.Banana,
            TransactionType = TransactionType.Deposit
        };

        await _assetManager.Deposit(asset.Username, asset.Unit, asset.AssetType);

        _mockContext.Assets.Single(x => x.Username == asset.Username && x.AssetType == asset.AssetType).Should()
            .BeEquivalentTo(asset);
    }
}