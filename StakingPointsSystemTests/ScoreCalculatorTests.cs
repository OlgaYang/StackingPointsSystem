using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.Core;
using StakingPointsSystem;
using StakingPointsSystem.Models;
using StakingPointsSystem.Services;

namespace StakingPointsSystemTests;

public class ScoreCalculatorTests
{
    [Test]
    public async Task calculate_a_user_score_with_deposit()
    {
        var options = new DbContextOptionsBuilder<StakingPointsDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        var mockContext = new StakingPointsDbContext(options);

        var userId = 1;

        mockContext.Assets.AddRange(new List<Asset>()
        {
            new()
            {
                UserId = userId,
                TransactionType = TransactionType.Deposit,
                AssetType = AssetType.Banana,
                Unit = 4,
                CreatedTime = new DateTime(2024, 8, 25, 23, 59, 10)
            }
        });
        await mockContext.SaveChangesAsync();

        var scoreCalculator = new ScoreCalculator(mockContext);
        await scoreCalculator.Calculate(new DateTime(2024, 8, 26));

        var userScore = mockContext.UserScores.Single(x => x.UserId == userId);
        userScore.TotalScore.Should().Be(50 * 20 * 4); // 50*20*4
    }
}