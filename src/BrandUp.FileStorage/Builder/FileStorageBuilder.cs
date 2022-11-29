using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.FileStorage.Builder
{
    public class FileStorageBuilder : IFileStorageBuilder
    {
        public IServiceCollection Services { get; set; }

        public IDictionary<Type, PropertyInfo[]> Properties { get; init; } // Type is file type
        public IDictionary<Type, ConfigurationCache> ConfigurationCache { get; init; } // Type is config type

        public FileStorageBuilder(IServiceCollection services)
        {
            Properties = new Dictionary<Type, PropertyInfo[]>();
            Services = services ?? throw new ArgumentNullException(nameof(services));

            ConfigurationCache = new Dictionary<Type, ConfigurationCache>();

            Services.AddScoped<IFileStorageFactory, FileStorageFactory>();
            Services.AddSingleton<IFileStorageBuilder, FileStorageBuilder>(f =>
            {
                return this;
            }); ;
        }

        public FileStorageBuilder AddConfiguration<TConfig>(Type storageType, TConfig configuration) where TConfig : class
        {
            var configType = typeof(TConfig);

            var cache = new ConfigurationCache(storageType, configType, configuration);

            if (!ConfigurationCache.TryAdd(configType, cache))
                throw new ArgumentException($"Configuration for {storageType} already exist");

            return this;
        }

        public FileStorageBuilder AddFileToStorage<TFile>(object configuration) where TFile : class, new()
        {
            var type = typeof(TFile);
            var configType = configuration.GetType();

            if (ConfigurationCache.TryGetValue(configType, out var cache))
                cache.Add(type, configuration);

            var properties = type.GetProperties();
            if (!Properties.TryGetValue(type, out _))
                if (!Properties.TryAdd(type, properties))
                    throw new Exception($"Unknown error on adding data to dictionary");

            return this;
        }
    }

    public interface IFileStorageBuilder
    {
        public IServiceCollection Services { get; set; }

        public IDictionary<Type, PropertyInfo[]> Properties { get; }
        public IDictionary<Type, ConfigurationCache> ConfigurationCache { get; }

        FileStorageBuilder AddFileToStorage<TFile>(object configuration) where TFile : class, new();
        FileStorageBuilder AddConfiguration<TConfig>(Type storageType, TConfig configuration) where TConfig : class;

    }
}