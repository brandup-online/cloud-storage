using BrandUp.FileStorage.Internals.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace BrandUp.FileStorage.Internals
{
    internal class StorageContextFactory : IDisposable
    {
        readonly IServiceProvider rootServiceProvider;
        readonly IStorageProviderConfiguration providerConfiguration;
        readonly ConcurrentDictionary<string, IStorageProvider> providers = new();

        public StorageContextFactory(IServiceProvider rootServiceProvider)
        {
            this.rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException(nameof(rootServiceProvider));
            providerConfiguration = rootServiceProvider.GetRequiredService<IStorageProviderConfiguration>();
        }

        public TContext Resolve<TContext>(IServiceProvider serviceProvider, string configurationName, ContextConfiguration contextConfiguration)
            where TContext : FileStorageContext
        {
            var provider = providers.GetOrAdd(configurationName, (c) =>
            {
                var configuration = providerConfiguration.Resolve(configurationName);
                return configuration.Create(rootServiceProvider);
            });

            var contextInfo = StorageContextTypes.GetContextInfo<TContext>();

            var context = contextInfo.CreateInstance<TContext>(serviceProvider, provider);
            context.Initialize(provider, contextInfo, contextConfiguration);
            return context;
        }

        #region IDisposable members

        void IDisposable.Dispose()
        {
            foreach (var providerDisposable in providers.Values.OfType<IDisposable>())
                providerDisposable.Dispose();
        }

        #endregion
    }
}