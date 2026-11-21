using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Application.Features.Analytics;
using FinanceTracker.Application.Features.Anomalies;
using FinanceTracker.Application.Features.Budgets;
using FinanceTracker.Application.Features.Forecasting;
using FinanceTracker.Application.Features.Categorization;
using FinanceTracker.Application.Features.Categorization.Ml;
using FinanceTracker.Application.Features.Imports;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Infrastructure.DerivedData;
using FinanceTracker.Infrastructure.MachineLearning;
using FinanceTracker.Infrastructure.Authentication;
using FinanceTracker.Infrastructure.Identity;
using FinanceTracker.Infrastructure.Persistence;
using FinanceTracker.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FinanceTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Hiányzik a ConnectionStrings:Default beállítás.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionQueries, TransactionQueries>();
        services.AddScoped<IAnalyticsQueries, AnalyticsQueries>();
        services.AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>();

        // UC-25..UC-28: anomáliák, költségkeretek, előrejelzés. A küszöbök és ablakok a konfigurációból jönnek.
        services.Configure<AnomalySettings>(configuration.GetSection(AnomalySettings.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AnomalySettings>>().Value);
        services.Configure<BudgetSettings>(configuration.GetSection(BudgetSettings.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<BudgetSettings>>().Value);
        services.Configure<ForecastSettings>(configuration.GetSection(ForecastSettings.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<ForecastSettings>>().Value);

        services.AddScoped<IAnomalyQueries, AnomalyQueries>();
        services.AddScoped<IAnomalyRepository, AnomalyRepository>();
        services.AddScoped<IBudgetQueries, BudgetQueries>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IForecastQueries, ForecastQueries>();
        services.AddScoped<IForecastRepository, ForecastRepository>();
        services.AddScoped<IImportRepository, ImportRepository>();
        services.AddScoped<ICategorizationRuleRepository, CategorizationRuleRepository>();
        services.AddScoped<IDerivedDataInvalidator, LoggingDerivedDataInvalidator>();

        // UC-17..UC-20: visszajelzések, modellverziók, ML.NET alapú tanítás és predikció.
        services.Configure<MlSettings>(configuration.GetSection(MlSettings.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<MlSettings>>().Value);

        services.AddScoped<ICategorizationFeedbackRepository, CategorizationFeedbackRepository>();
        services.AddScoped<IMlModelRepository, MlModelRepository>();
        services.AddScoped<ITrainingDataSource, TrainingDataSource>();

        services.AddSingleton<MlModelStore>();
        services.AddSingleton<IMlModelStore, MlModelStorePort>();
        services.AddSingleton<IMlModelTrainer, MlNetModelTrainer>();
        services.AddSingleton<IMlModelRuntime, MlNetModelRuntime>();

        return services;
    }
}
