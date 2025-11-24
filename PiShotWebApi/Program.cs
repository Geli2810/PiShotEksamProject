using PiShotProject;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddSingleton<Class1Repo>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Disable HTTPS redirection for Azure
// app.UseHttpsRedirection();


app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
