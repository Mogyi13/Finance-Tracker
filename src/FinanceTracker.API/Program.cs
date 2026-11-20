using System.Text;
using System.Text.Json.Serialization;
using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Infrastructure;
using FinanceTracker.Infrastructure.Authentication;
using FinanceTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- Szolgáltatások
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Az enumok névvel (például "Income") szerepelnek a JSON-ban, a Web is így használja őket.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
{
    throw new InvalidOperationException("A Jwt:Key beállítás hiányzik, vagy rövidebb 32 bájtnál.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // A "sub" claim maradjon "sub" (ne képeződjön át a hosszú claim típusra).
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "name"
        };
    });

// UC-20: a modellkezelő végpontok csak a konfigurációban felsorolt üzemeltetőknek érhetők el (alapértelmezésben senkinek).
var modelAdministrators = builder.Configuration.GetSection(AuthorizationPolicies.ModelAdministratorEmailsKey).Get<string[]>() ?? [];
builder.Services.AddAuthorization(options => options.AddModelAdministratorPolicy(modelAdministrators));

var app = builder.Build();

// --- Middleware
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Konténerben csak HTTP-n figyelünk, ott nincs HTTPS-átirányítás.
if (!string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

if (app.Configuration.GetValue("Database:AutoMigrate", app.Environment.IsDevelopment()))
{
    await DatabaseMigrator.MigrateAsync(app.Services);
}

app.Run();
