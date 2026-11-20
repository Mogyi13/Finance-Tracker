using FinanceTracker.Application.Features.Analytics.ComparePeriods;
using FinanceTracker.Application.Features.Analytics.GetCategoryBreakdown;
using FinanceTracker.Application.Features.Analytics.GetOverview;
using FinanceTracker.Application.Features.Analytics.Recurring;
using FinanceTracker.Application.Features.Authentication.Login;
using FinanceTracker.Application.Features.Authentication.Register;
using FinanceTracker.Application.Features.Categories.CreateCategory;
using FinanceTracker.Application.Features.Categorization;
using FinanceTracker.Application.Features.Categorization.CorrectCategory;
using FinanceTracker.Application.Features.Categorization.CreateRule;
using FinanceTracker.Application.Features.Categorization.DeleteRule;
using FinanceTracker.Application.Features.Categorization.GetRules;
using FinanceTracker.Application.Features.Categorization.GetSuggestions;
using FinanceTracker.Application.Features.Categorization.Ml;
using FinanceTracker.Application.Features.Categorization.PredictCategory;
using FinanceTracker.Application.Features.Categorization.SetRuleActive;
using FinanceTracker.Application.Features.Categorization.SetRulePriority;
using FinanceTracker.Application.Features.Categorization.Training;
using FinanceTracker.Application.Features.Categorization.UpdateRule;
using FinanceTracker.Application.Features.Categories.DeleteCategory;
using FinanceTracker.Application.Features.Categories.GetCategories;
using FinanceTracker.Application.Features.Categories.UpdateCategory;
using FinanceTracker.Application.Features.Imports.GetImport;
using FinanceTracker.Application.Features.Imports.GetImportErrors;
using FinanceTracker.Application.Features.Imports.GetImports;
using FinanceTracker.Application.Features.Imports.ImportCsv;
using FinanceTracker.Application.Features.Imports.PreviewImport;
using FinanceTracker.Application.Features.Profile.GetProfile;
using FinanceTracker.Application.Features.Profile.UpdateProfile;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Application.Features.Transactions.CreateTransaction;
using FinanceTracker.Application.Features.Transactions.DeleteTransaction;
using FinanceTracker.Application.Features.Transactions.GetTransaction;
using FinanceTracker.Application.Features.Transactions.GetTransactions;
using FinanceTracker.Application.Features.Transactions.UpdateTransaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FinanceTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<GetProfileHandler>();
        services.AddScoped<UpdateProfileHandler>();

        services.AddScoped<GetCategoriesHandler>();
        services.AddScoped<CreateCategoryHandler>();
        services.AddScoped<UpdateCategoryHandler>();
        services.AddScoped<DeleteCategoryHandler>();

        services.AddScoped<CreateTransactionHandler>();
        services.AddScoped<GetTransactionsHandler>();
        services.AddScoped<GetTransactionHandler>();
        services.AddScoped<UpdateTransactionHandler>();
        services.AddScoped<DeleteTransactionHandler>();

        services.AddScoped<PreviewImportHandler>();
        services.AddScoped<ImportCsvHandler>();
        services.AddScoped<GetImportsHandler>();
        services.AddScoped<GetImportHandler>();
        services.AddScoped<GetImportErrorsHandler>();

        // UC-15, UC-18, UC-19: az automatikus kategorizálás sorrendje: előbb a felhasználó szabályai, utána az ML-modell.
        // Az MlSettings alapértelmezett értékeit az Infrastructure a konfigurációból felülírja.
        services.TryAddSingleton<MlSettings>();
        services.AddScoped<RuleBasedTransactionCategorizer>();
        services.AddScoped<MlCategoryService>();
        services.AddScoped<CategoryPredictionService>();
        services.AddScoped<ITransactionCategorizer, CompositeTransactionCategorizer>();

        // UC-17, UC-18, UC-19
        services.AddScoped<CorrectCategoryHandler>();
        services.AddScoped<PredictCategoryHandler>();
        services.AddScoped<GetSuggestionsHandler>();

        // UC-21..UC-24: pénzügyi elemzések és ismétlődő kiadások felismerése
        services.AddScoped<GetOverviewHandler>();
        services.AddScoped<GetCategoryBreakdownHandler>();
        services.AddScoped<ComparePeriodsHandler>();
        services.AddScoped<GetRecurringHandler>();
        services.AddScoped<DetectRecurringHandler>();

        // UC-20: modell tanítása, kiértékelése, aktiválása
        services.AddScoped<TrainModelHandler>();
        services.AddScoped<GetModelVersionsHandler>();
        services.AddScoped<GetModelVersionHandler>();
        services.AddScoped<ActivateModelHandler>();
        services.AddScoped<DeactivateModelHandler>();

        services.AddScoped<GetRulesHandler>();
        services.AddScoped<CreateRuleHandler>();
        services.AddScoped<UpdateRuleHandler>();
        services.AddScoped<SetRuleActiveHandler>();
        services.AddScoped<SetRulePriorityHandler>();
        services.AddScoped<DeleteRuleHandler>();
        return services;
    }
}
