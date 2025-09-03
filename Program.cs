using Microsoft.EntityFrameworkCore;
using SplitwiseAPI.Data;
using Pomelo.EntityFrameworkCore.MySql;
using SplitwiseAPI.Repositories.Interfaces;
using SplitwiseAPI.Repositories.Repositories;
using SplitwiseAPI.Services.Interfaces;
using SplitwiseAPI.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Bu satırı ekleyin - external IP'den erişim için
builder.WebHost.UseUrls("http://0.0.0.0:5089");

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework with MySQL (Pomelo)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IUserExpenseRepository, UserExpenseRepository>();

// Register Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IUserExpenseService, UserExpenseService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Splitwise API",
        Version = "v1",
        Description = "API for expense sharing application like Splitwise"
    });
});

// Configure CORS (if needed for frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Splitwise API V1");
        c.RoutePrefix = string.Empty; // Swagger UI will be available at root URL
    });
}

// HTTPS yönlendirmesini kaldırın - mobil erişim için HTTP kullanacağız
// app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Ensure database is created (for development)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

app.Run();