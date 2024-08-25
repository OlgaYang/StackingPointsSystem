using Microsoft.AspNetCore.Mvc;

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
    public List<UserEntity> Test()
    {
        return _dbContext.UserEntities.Select(x => x).ToList();
    }
}