using BrandUp.FileStorage.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Builder
{
    public class FileStorageBuilder : IFileStorageBuilder, IStorageProviderConfiguration
    {
        readonly Dictionary<string, ProviderConfiguration> providerConfigurations = new();

        public FileStorageBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));

            AddCoreServices();
        }

        void AddCoreServices()
        {
            var services = Services;

            services.AddSingleton<IStorageProviderConfiguration>(this); // Конфигурациия провайдеров хранения файлов
            services.AddSingleton<StorageContextFactory>(); // Фабрика контекстов файловых хранилищ
        }

        #region IFileStorageBuilder members

        public IServiceCollection Services { get; set; }

        public IFileStorageBuilder AddStorageProvider<TStorageProvider, TOptions>(string configurationName, Action<TOptions> configureOptions)
            where TStorageProvider : class, IStorageProvider
            where TOptions : class, new()
        {
            if (configurationName == null)
                throw new ArgumentNullException(nameof(configurationName));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            var options = new TOptions();
            configureOptions(options);

            var providerConfiguration = new ProviderConfiguration(typeof(TStorageProvider), options);
            providerConfigurations[configurationName] = providerConfiguration;

            return this;
        }

        #endregion

        #region IFileStorageProviderConfiguration members

        ProviderConfiguration IStorageProviderConfiguration.Resolve(string configurationName)
        {
            if (!providerConfigurations.TryGetValue(configurationName, out var providerConfiguration))
                throw new InvalidOperationException("Configuration provider not found.");

            return providerConfiguration;
        }

        //public IFileStorageBuilder AddCollectionConfiguration(string collectionName, Action<string> options)
        //{

        //}

        #endregion
    }

    public interface IFileStorageBuilder
    {
        IServiceCollection Services { get; set; }

        IFileStorageBuilder AddStorageProvider<TStorageProvider, TOptions>(string configurationName, Action<TOptions> configureOptions)
            where TStorageProvider : class, IStorageProvider
            where TOptions : class, new();

        //IFileStorageBuilder AddCollectionConfiguration(string collectionName, Action<string> options);
    }
}