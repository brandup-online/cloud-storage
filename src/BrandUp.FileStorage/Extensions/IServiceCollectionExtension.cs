using BrandUp.FileStorage.Builder;
using BrandUp.FileStorage.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage
{
    public static class IServiceCollectionExtension
    {
        public static IFileStorageBuilder AddFileStorage(this IServiceCollection services)
        {
            return new FileStorageBuilder(services);
        }

        public static IServiceCollection AddFileContext<TContext>(this IServiceCollection services, string configurationName)
            where TContext : FileStorageContext
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configurationName == null)
                throw new ArgumentNullException(nameof(configurationName));

            StorageContextTypes.RegisterContextType<TContext>();

            services.AddScoped(serviceProvider =>
            {
                var factory = serviceProvider.GetRequiredService<StorageContextFactory>();
                return factory.Resolve<TContext>(serviceProvider, configurationName);
            });

            return services;
        }
    }
}