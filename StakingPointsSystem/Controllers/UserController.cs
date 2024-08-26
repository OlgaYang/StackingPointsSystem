using Microsoft.AspNetCore.Mvc;
using StakingPointsSystem.Models;

namespace StakingPointsSystem.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly StakingPointsDbContext _dbContext;

    public UserController(StakingPointsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<List<User>> Test()
    {
        var random = new Random();
        for (int i = 1000; i < 1000000; i++)
        {
            var asset = new Asset
            {
                UserId = i,
                TransactionType = TransactionType.Deposit,
                Unit = random.Next(0, 100),
                CreatedTime = DateTime.Now,
                AssetType = (AssetType)random.Next(1,4),
            };
            
            _dbContext.Assets.Add(asset); 
        }

        await _dbContext.SaveChangesAsync();
        return _dbContext.Users.Select(x => x).ToList();
    }
}