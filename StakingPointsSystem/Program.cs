using Microsoft.EntityFrameworkCore;
using StakingPointsSystem;
using StakingPointsSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHostedService<TimedHostedService>();
builder.Services.AddScoped<ScoreCalculator>();
builder.Services.AddDbContext<StakingPointsDbContext>(options =>
    options.UseSqlServer("Server=DESKTOP-MN3CUAK;Database=StakingPoints;Trusted_Connection=True;"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();