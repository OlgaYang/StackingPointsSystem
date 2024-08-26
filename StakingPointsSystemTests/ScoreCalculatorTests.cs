using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StakingPointsSystem;
using StakingPointsSystem.Models;
using StakingPointsSystem.Services;

namespace StakingPointsSystemTests;

public class ScoreCalculatorTests
{
    private StakingPointsDbContext _mockContext;
    private ScoreCalculator _scoreCalculator;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<StakingPointsDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _mockContext = new StakingPointsDbContext(options);
        _scoreCalculator = new ScoreCalculator(_mockContext);
    }

    [Test]
    public async Task no_deposit_and_no_score()
    {
        var userId = 1;
        _mockContext.Users.AddRange(new User { UserId = userId });
        await _mockContext.SaveChangesAsync();
        
        await _scoreCalculator.Calculate(GetTime(59, 50));
        
        var userScore = _mockContext.UserScores.SingleOrDefault(x => x.UserId == userId);
        userScore.Should().BeNull();
    }


    [Test]
    public async Task first_deposit_and_no_score()
    {
        var userId = 1;
        _mockContext.Users.AddRange(new User { UserId = userId });
        _mockContext.Assets.AddRange(new List<Asset>
        {
            new()
            {
                UserId = userId,
                TransactionType = TransactionType.Deposit,
                AssetType = AssetType.Banana,
                Unit = 4,
                CreatedTime = GetTime(59, 10)
            }
        });
        _mockContext.Balances.Add(new Balance { UserId = userId, AssetType = AssetType.Banana, Unit = 4 });

        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));


        var userScore = _mockContext.UserScores.Single(x => x.UserId == userId);
        userScore.TotalScore.Should().Be(40 * 20 * 4); // second * based score * unit 
    }

    [Test]
    public async Task no_asset_changed_after_last_update()
    {
        var userId = 1;
        _mockContext.Users.AddRange(new User { UserId = userId });
        _mockContext.Assets.AddRange(new List<Asset>()
        {
            new()
            {
                UserId = userId,
                TransactionType = TransactionType.Deposit,
                AssetType = AssetType.Banana,
                Unit = 4,
                CreatedTime = GetTime(59, 10)
            }
        });
        _mockContext.Balances.Add(new Balance { UserId = userId, AssetType = AssetType.Banana, Unit = 4 });
        _mockContext.UserScores.Add(new UserScore
        {
            UserId = userId,
            TotalScore = 100,
            LastUpdatedTime = GetTime(59, 30)
        });
        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        var userScore = _mockContext.UserScores.Single(x => x.UserId == userId);
        userScore.TotalScore.Should().Be(100 + 20 * 20 * 4); //previous score + second * based score * unit 
    }

    [TearDown]
    public void TearDown()
    {
        _mockContext.Database.EnsureDeleted();
        _mockContext.Dispose();
    }

    private static DateTime GetTime(int minute, int second)
    {
        return new DateTime(2024, 8, 25, 23, minute, second);
    }
}