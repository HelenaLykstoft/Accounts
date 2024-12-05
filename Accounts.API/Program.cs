using Accounts.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using Accounts.Core.Ports.Driven;
using Accounts.Infrastructure.Repositories;
using Accounts.Core.Models;
using Accounts.Core.Validators;
using Accounts.Core.Services;
using Accounts.Core.Ports.Driving;


var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Load ENV file
Env.Load("Accounts/.env");
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

builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IContactInfoRepository, ContactInfoRepository>();
builder.Services.AddScoped<ILoginInfoRepository, LoginInfoRepository>();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<ISessionStore, SessionStore>();
builder.Services.AddScoped<ITransactionHandler, TransactionHandler>();

// Validation
services.AddFluentValidationAutoValidation(); 
services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>(); 
services.AddTransient<IValidator<RegisterUserCommand>, RegisterUserValidator>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Add user types if they dont exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.AddDefaultUserTypesAsync();
    var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
    await accountService.SeedAdminUserAsync();
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

app.Run("http://*:80");
