using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace StakingPointsSystem.Controllers;

[ApiController]
[Route("[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly StakingPointsDbContext _dbContext;

    public LeaderboardController(StakingPointsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<List<LeaderBoardResponse>> Index()
    {
        return (await _dbContext.UserScores
                    .Join(_dbContext.Users, x => x.UserId, y => y.UserId,
                        (score, user) => new { score, user })
                    .OrderByDescending(x => x.score.TotalScore)
                    .Take(300)
                    .ToListAsync())
                .Select((x, i) => new LeaderBoardResponse
                {
                    Name = x.user.Name.ToString(),
                    Rank = i + 1,
                    Scores = x.score.TotalScore
                }).ToList();
    }
}

public class LeaderBoardResponse
{
    public string Name { get; set; }
    public int Rank { get; set; }
    public decimal Scores { get; set; }
}