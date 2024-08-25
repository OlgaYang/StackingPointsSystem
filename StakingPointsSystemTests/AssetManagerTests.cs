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

        var asset = new Asset()
        {
            Username = "olga",
            TransactionType = TransactionType.Deposit,
            Unit = 4,
            CreatedTime = DateTime.Now,
            AssetType = AssetType.Banana
        };

        await assetManager.Deposit(asset);
        mockContext.Assets.Any(x => x.Username == asset.Username && x.CreatedTime == asset.CreatedTime).Should()
            .BeTrue();
    }
}