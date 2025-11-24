using Microsoft.OpenApi;
using MiniNetwork.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();                
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MiniNetwork API",
        Version = "v1"
    });
});
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

// Bật Swagger UI luôn cho dễ dev
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiniNetwork API v1");
    c.RoutePrefix = "swagger"; // => /swagger
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();  // map tất cả controller

app.Run();
