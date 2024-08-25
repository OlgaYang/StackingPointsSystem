using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StakingPointsSystem;
using StakingPointsSystem.Models;
using StakingPointsSystem.Services;

namespace StakingPointsSystemTests;

public class AssetManagerTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task user_deposit_a_banana()
    {
        var options = new DbContextOptionsBuilder<StakingPointsDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        var mockContext = new StakingPointsDbContext(options);
        var assetManager = new AssetManager(mockContext);

        var asset = new
        {
            Username = "olga",
            Unit = 4,
            AssetType = AssetType.Banana,
            TransactionType = TransactionType.Deposit
        };

        await assetManager.Deposit(asset.Username, asset.Unit, asset.AssetType);

        mockContext.Assets.Single(x => x.Username == asset.Username && x.AssetType == asset.AssetType).Should()
            .BeEquivalentTo(asset);
    }
}