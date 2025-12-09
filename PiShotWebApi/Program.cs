using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
using PiShotProject.Interfaces;
using PiShotProject.Repositories;
using PiShotProject.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IScoreRepository, ScoreRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PiShotDBContext>(opts => opts.UseSqlServer(cs));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.MapControllers();

app.Run();