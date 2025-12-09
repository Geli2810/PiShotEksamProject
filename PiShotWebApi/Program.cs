using Microsoft.EntityFrameworkCore;
using PiShotProject.ClassDB;
using PiShotProject.Interfaces;
using PiShotProject.Repositories;
using PiShotProject.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (vi registrerer bare CORS, men bruger inline policy senere)
builder.Services.AddCors();

// DI
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IScoreRepository, ScoreRepository>();

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PiShotDBContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Routing -> CORS -> Auth -> Controllers
app.UseRouting();

// *** VIGTIGT: global CORS-policy inline ***
app.UseCors(cors => cors
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.UseAuthorization();

app.MapControllers();

app.Run();
