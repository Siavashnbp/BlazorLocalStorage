using BlazorLocalStorageManager.Interfaces;
using BlazorLocalStorageManager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorLocalStorageManager.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBrowserStorage(this IServiceCollection services)
        {
            services.AddScoped<ILocalStorageService, LocalStorageService>();

            return services;
        }
    }
}
