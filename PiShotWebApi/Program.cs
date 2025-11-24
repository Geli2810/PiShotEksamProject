using PiShotProject;

var builder = WebApplication.CreateBuilder(args);

<<<<<<<<< Temporary merge branch 1
// Add services to the container.

=========
builder.Services.AddControllers();
>>>>>>>>> Temporary merge branch 2
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
=========
builder.Services.AddControllers();
>>>>>>>>> Temporary merge branch 2
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


app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
