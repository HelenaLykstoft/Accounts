using Accounts.API.DTO;
using Accounts.API.Kafka;
using Accounts.API.Services;
using Accounts.Infrastructure.Kafka;
using Accounts.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Description = "Enter 'Bearer {your token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add the producer service as singletons:
builder.Services.AddSingleton<KafkaProducer>();
// Add the kafka consumer service as a hosted service (background service that runs for the lifetime of the application):
builder.Services.AddHostedService<KafkaConsumer>();
builder.Services.AddSingleton<TestService>();
builder.Services.AddScoped<AccountService>();

// Session singleton
builder.Services.AddSingleton<SessionStore>();

// Validation
services.AddFluentValidationAutoValidation();
services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
services.AddTransient<IValidator<RegisterUserRequest>, RegisterUserValidator>();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Add user types if they dont exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.AddDefaultUserTypesAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<TokenValidationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
