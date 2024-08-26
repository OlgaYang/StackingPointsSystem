using Microsoft.AspNetCore.Mvc;
using StakingPointsSystem.Services;

namespace StakingPointsSystem.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public Task Calculate()
    {
        // var scoreCalculator = new ScoreCalculator();

        // await scoreCalculator.Calculate();
        
        return Task.CompletedTask;
    }
}