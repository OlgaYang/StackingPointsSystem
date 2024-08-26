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

    [Test]
    public async Task has_deposit_after_last_update()
    {
        var userId = 1;
        _mockContext.Users.AddRange(new User { UserId = userId });
        _mockContext.UserScores.Add(new UserScore
        {
            UserId = userId,
            TotalScore = 100,
            LastUpdatedTime = GetTime(59, 10)
        });
        _mockContext.Assets.AddRange(new List<Asset>()
        {
            new()
            {
                UserId = userId,
                TransactionType = TransactionType.Deposit,
                AssetType = AssetType.Banana,
                Unit = 4,
                CreatedTime = GetTime(59, 40)
            }
        });
        _mockContext.Balances.Add(new Balance { UserId = userId, AssetType = AssetType.Banana, Unit = 6 });

        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        var userScore = _mockContext.UserScores.Single(x => x.UserId == userId);
        userScore.TotalScore.Should().Be(100 + (40 * 20 * 6) - (30 * 20 * 4));
    }

    [Test]
    public async Task has_withdraw_after_last_update()
    {
        var userId = 1;
        _mockContext.Users.AddRange(new User { UserId = userId });
        _mockContext.UserScores.Add(new UserScore
        {
            UserId = userId,
            TotalScore = 100,
            LastUpdatedTime = GetTime(59, 10)
        });
        _mockContext.Assets.AddRange(new List<Asset>()
        {
            new()
            {
                UserId = userId,
                TransactionType = TransactionType.Withdraw,
                AssetType = AssetType.Banana,
                Unit = 4,
                CreatedTime = GetTime(59, 40)
            }
        });
        _mockContext.Balances.Add(new Balance { UserId = userId, AssetType = AssetType.Banana, Unit = 6 });

        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        var userScore = _mockContext.UserScores.Single(x => x.UserId == userId);
        userScore.TotalScore.Should().Be(100 + (40 * 20 * 6) + (30 * 20 * 4));
    }


    [Test]
    public async Task has_different_type_deposit_after_last_update()
    {
        var userId = 1;
        _mockContext.Users.AddRange(new User { UserId = userId });
        _mockContext.UserScores.Add(new UserScore
        {
            UserId = userId,
            TotalScore = 100,
            LastUpdatedTime = GetTime(59, 10)
        });
        _mockContext.Assets.AddRange(new List<Asset>()
        {
            new()
            {
                AssetType = AssetType.Banana, Unit = 100, CreatedTime = GetTime(59, 40), UserId = userId,
                TransactionType = TransactionType.Deposit
            },
            new()
            {
                AssetType = AssetType.Kiwi, Unit = 10, CreatedTime = GetTime(59, 40), UserId = userId,
                TransactionType = TransactionType.Deposit
            },
            new()
            {
                AssetType = AssetType.Apple, Unit = 1, CreatedTime = GetTime(59, 40), UserId = userId,
                TransactionType = TransactionType.Deposit
            }
        });
        _mockContext.Balances.Add(new Balance { UserId = userId, AssetType = AssetType.Banana, Unit = 110 });
        _mockContext.Balances.Add(new Balance { UserId = userId, AssetType = AssetType.Kiwi, Unit = 11 });
        _mockContext.Balances.Add(new Balance { UserId = userId, AssetType = AssetType.Apple, Unit = 2 });

        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        var userScore = _mockContext.UserScores.Single(x => x.UserId == userId);
        var expect = 100 + (110 * 20 * 40) + (11 * 10 * 40) + (2 * 100 * 40) -
                     (100 * 20 * 30) - (10 * 10 * 30) - (1 * 100 * 30);
        userScore.TotalScore.Should().Be(expect); // previous score + balance - diff
    }


    [Test]
    public async Task two_user_has_deposit()
    {
        var userId = 1;
        var userId2 = 2;
        _mockContext.Users.AddRange(new User { UserId = userId }, new User() { UserId = userId2 });
        _mockContext.UserScores.Add(new UserScore
        {
            UserId = userId,
            TotalScore = 100,
            LastUpdatedTime = GetTime(59, 10)
        });
        _mockContext.Assets.AddRange(new List<Asset>()
        {
            new()
            {
                UserId = userId, TransactionType = TransactionType.Deposit, AssetType = AssetType.Banana, Unit = 4,
                CreatedTime = GetTime(59, 40)
            },
            new()
            {
                UserId = userId2, TransactionType = TransactionType.Deposit, AssetType = AssetType.Banana, Unit = 5,
                CreatedTime = GetTime(59, 40)
            },
        });
        _mockContext.Balances.Add(new Balance { UserId = userId, AssetType = AssetType.Banana, Unit = 6 });
        _mockContext.Balances.Add(new Balance { UserId = userId2, AssetType = AssetType.Banana, Unit = 5 });

        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        _mockContext.UserScores.Single(x => x.UserId == userId).TotalScore
            .Should().Be(100 + (40 * 20 * 6) - (30 * 20 * 4));

        _mockContext.UserScores.Single(x => x.UserId == userId2).TotalScore
            .Should().Be((10 * 20 * 5) - 0); 
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