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
    private readonly int _userId = 1;

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
        _mockContext.Users.AddRange(new User { UserId = _userId });
        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        var userScore = _mockContext.UserScores.SingleOrDefault(x => x.UserId == _userId);
        userScore.Should().BeNull();
    }


    [Test]
    // 23:59:10 Deposit 4 Bananas
    // 23:59:50 Run update , Balance is 4 bananas =>
    // result =  40 second * 20 based score * 4 unit 
    public async Task first_deposit_and_no_score()
    {
        _mockContext.Users.AddRange(new User { UserId = _userId });
        _mockContext.Assets.AddRange(new List<Asset>
        {
            GetDepositAsset(_userId, AssetType.Banana, 4, GetTime(59, 10))
        });
        _mockContext.Balances.Add(new Balance { UserId = _userId, AssetType = AssetType.Banana, Unit = 4 });
        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        var userScore = _mockContext.UserScores.Single(x => x.UserId == _userId);
        userScore.TotalScore.Should().Be(40 * 20 * 4); // 40 second * 20 based score * 4 unit 
    }


    [Test]
    // 23:59:10 Deposit 4 Bananas
    // 23:59:30 Updated Score is 100
    // 23:59:50 Run update , Balance is 4 bananas =>
    // result = 100 previous score + 20 second * 20 based score * 4 unit 
    public async Task no_asset_changed_after_last_update()
    {
        _mockContext.Users.AddRange(new User { UserId = _userId });
        _mockContext.Assets.AddRange(new List<Asset>()
        {
            GetDepositAsset(_userId, AssetType.Banana, 4, GetTime(59, 10)),
        });
        _mockContext.Balances.Add(new Balance { UserId = _userId, AssetType = AssetType.Banana, Unit = 4 });
        _mockContext.UserScores.Add(new UserScore
        {
            UserId = _userId,
            TotalScore = 100,
            LastUpdatedTime = GetTime(59, 30)
        });
        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        var userScore = _mockContext.UserScores.Single(x => x.UserId == _userId);
        userScore.TotalScore.Should().Be(100 + 20 * 20 * 4);
    }

    [Test]
    // 23:59:10 Updated Score is 100
    // 23:59:40 Deposit 4 Bananas 
    // 23:59:50 Run update, Balance is 6 bananas =>
    // result = 100 previous score + (40 second * 20 based score * 6 unit) - before deposit score(30 second * 20 based score * 4 unit)
    public async Task has_deposit_after_last_update()
    {
        _mockContext.Users.AddRange(new User { UserId = _userId });
        _mockContext.UserScores.Add(new UserScore
            { UserId = _userId, TotalScore = 100, LastUpdatedTime = GetTime(59, 10) });
        _mockContext.Assets.AddRange(new List<Asset>()
        {
            GetDepositAsset(_userId, AssetType.Banana, 4, GetTime(59, 40)),
        });
        _mockContext.Balances.Add(new Balance { UserId = _userId, AssetType = AssetType.Banana, Unit = 6 });
        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        var userScore = _mockContext.UserScores.Single(x => x.UserId == _userId);
        userScore.TotalScore.Should().Be(100 + (40 * 20 * 6) - (30 * 20 * 4));
    }

    [Test]
    // 23:59:10 Updated Score is 100
    // 23:59:40 Withdraw 4 Bananas 
    // 23:59:50 Run update, Balance is 6 bananas =>
    // result = 100 previous score + (40 second * 20 based score * 6 unit) + before deposit score(30 second * 20 based score * 4 unit)
    public async Task has_withdraw_after_last_update()
    {
        _mockContext.Users.AddRange(new User { UserId = _userId });
        _mockContext.UserScores.Add(new UserScore
            { UserId = _userId, TotalScore = 100, LastUpdatedTime = GetTime(59, 10) });
        _mockContext.Assets.AddRange(new List<Asset>()
        {
            GetWithdrawAsset(_userId, AssetType.Banana, 4, GetTime(59, 40)),
        });
        _mockContext.Balances.Add(new Balance { UserId = _userId, AssetType = AssetType.Banana, Unit = 6 });

        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        var userScore = _mockContext.UserScores.Single(x => x.UserId == _userId);
        userScore.TotalScore.Should().Be(100 + (40 * 20 * 6) + (30 * 20 * 4));
    }


    [Test]
    // 23:59:10 Updated Score is 100
    // 23:59:40 Deposit 100 Bananas, 10 Kiwis, 1 apple 
    // 23:59:50 Run update, Balance is 110 bananas, 11 kiwis, 2 apples =>
    // result = 100 previous score +
    //          bananas (40 second * 20 based score * 110 unit) - before deposit (30 second * 20 based score * 110 unit)
    //          kiwis (40 second * 10 based score * 11 unit) - before deposit (30 second * 10 based score * 10 unit)
    //          apples (40 second * 100 based score * 2 unit) - before deposit (30 second * 100 based score * 1 unit)
    public async Task has_different_type_deposit_after_last_update()
    {
        _mockContext.Users.AddRange(new User { UserId = _userId });
        _mockContext.UserScores.Add(new UserScore
        {
            UserId = _userId,
            TotalScore = 100,
            LastUpdatedTime = GetTime(59, 10)
        });
        _mockContext.Assets.AddRange(new List<Asset>()
        {
            GetDepositAsset(_userId, AssetType.Banana, 100, GetTime(59, 40)),
            GetDepositAsset(_userId, AssetType.Kiwi, 10, GetTime(59, 40)),
            GetDepositAsset(_userId, AssetType.Apple, 1, GetTime(59, 40)),
        });
        _mockContext.Balances.Add(new Balance { UserId = _userId, AssetType = AssetType.Banana, Unit = 110 });
        _mockContext.Balances.Add(new Balance { UserId = _userId, AssetType = AssetType.Kiwi, Unit = 11 });
        _mockContext.Balances.Add(new Balance { UserId = _userId, AssetType = AssetType.Apple, Unit = 2 });

        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        var userScore = _mockContext.UserScores.Single(x => x.UserId == _userId);
        var expect = 100 + (110 * 20 * 40) + (11 * 10 * 40) + (2 * 100 * 40) -
                     (100 * 20 * 30) - (10 * 10 * 30) - (1 * 100 * 30);
        userScore.TotalScore.Should().Be(expect); // previous score + balance - diff
    }


    [Test]
    // User 1
    // 23:59:10 Updated Score is 100
    // 23:59:40 Deposit 4 Bananas
    // 23:59:50 Run update, Balance is 6 
    // result = 100 previous score +
    //          bananas (40 second * 20 based score * 6 unit) - before deposit (30 second * 20 based score * 4 unit)
    // User 2
    // 23:59:40 Deposit 5 Bananas
    // 23:59:50 Run update, Balance is 5 
    // result = bananas (10 second * 20 based score * 5 unit)
    public async Task two_user_has_deposit()
    {
        var userId = 1;
        var userId2 = 2;
        _mockContext.Users.AddRange(new User { UserId = userId }, new User() { UserId = userId2 });
        _mockContext.UserScores.Add(new UserScore { UserId = userId, TotalScore = 100, LastUpdatedTime = GetTime(59, 10) });
        _mockContext.Assets.AddRange(new List<Asset>()
        {
            GetDepositAsset(userId, AssetType.Banana, 4, GetTime(59, 40)),
            GetDepositAsset(userId2, AssetType.Banana, 5, GetTime(59, 40)),
        });
        
        _mockContext.Balances.Add(new Balance { UserId = userId, AssetType = AssetType.Banana, Unit = 6 });
        _mockContext.Balances.Add(new Balance { UserId = userId2, AssetType = AssetType.Banana, Unit = 5 });

        await _mockContext.SaveChangesAsync();

        await _scoreCalculator.Calculate(GetTime(59, 50));

        _mockContext.UserScores.Single(x => x.UserId == userId).TotalScore
            .Should().Be(100 + (40 * 20 * 6) - (30 * 20 * 4));

        _mockContext.UserScores.Single(x => x.UserId == userId2).TotalScore
            .Should().Be((10 * 20 * 5));
    }

    [TearDown]
    public void TearDown()
    {
        _mockContext.Database.EnsureDeleted();
        _mockContext.Dispose();
    }

    private static Asset GetDepositAsset(int userId, AssetType assetType, int unit, DateTime createdTime)
    {
        return new()
        {
            UserId = userId,
            TransactionType = TransactionType.Deposit,
            AssetType = assetType,
            Unit = unit,
            CreatedTime = createdTime
        };
    }

    private static Asset GetWithdrawAsset(int userId, AssetType assetType, int unit, DateTime createdTime)
    {
        return new()
        {
            UserId = userId,
            TransactionType = TransactionType.Withdraw,
            AssetType = assetType,
            Unit = unit,
            CreatedTime = createdTime
        };
    }

    private static DateTime GetTime(int minute, int second)
    {
        return new DateTime(2024, 8, 25, 23, minute, second);
    }
}