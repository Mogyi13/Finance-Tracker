using FinanceTracker.Application.Features.Authentication.Login;
using FinanceTracker.Application.Features.Authentication.Register;
using FinanceTracker.Application.Features.Profile.GetProfile;
using FinanceTracker.Application.Features.Profile.UpdateProfile;
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
        return services;
    }
}
