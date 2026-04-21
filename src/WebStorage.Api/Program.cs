using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebStorage.Api;
using WebStorage.Api.ExceptionHandling;
using WebStorage.Application;
using WebStorage.Application.Auth;
using WebStorage.Infrastructure;
using WebStorage.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

builder.ConfigureAuthentication();
builder.ConfigureAuthorization();
builder.ConfigureCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Welcome to the WebStore!");

await DbSeeder.SeedAsync(app.Services, app.Configuration);

app.Run();
