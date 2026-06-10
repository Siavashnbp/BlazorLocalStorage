using BlazorLocalStorageManager.Abstraction;
using BlazorLocalStorageManager.Models;
using BlazorLocalStorageManager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorLocalStorageManager.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBrowserStorage(
            this IServiceCollection services,
            Action<BrowserStorageOptions>? configure = null)
        {
            if (configure is not null)
            {
                services.Configure(configure);
            }
            services.AddScoped<IBrowserStorage, BrowserStorage>();

            return services;
        }
    }
}
