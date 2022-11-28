using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.FileStorage.Builder
{
    public class FileStorageBuilder : IFileStorageBuilder
    {
        public IServiceCollection Services { get; set; }

        public IDictionary<string, PropertyInfo[]> Properties { get; init; }
        public IList<Type> Types { get; init; }
        public IDictionary<Type, ConfigurationCache> ConfigurationCache { get; init; }

        private const string configSuffix = "Configuration";
        private const string storageSuffix = "FileStorage";

        public FileStorageBuilder(IServiceCollection services)
        {
            Properties = new Dictionary<string, PropertyInfo[]>();
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Types = new List<Type>();

            ConfigurationCache = new Dictionary<Type, ConfigurationCache>();

            Services.AddScoped<IFileStorageFactory, FileStorageFactory>();
            Services.AddSingleton<IFileStorageBuilder, FileStorageBuilder>(f =>
            {
                return this;
            }); ;
        }

        public FileStorageBuilder AddFile<T>() where T : class, new()
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            Types.Add(type);
            if (!Properties.TryAdd(type.Name, properties))
                throw new ArgumentException($"Type {typeof(T)} already added in bulder");

            return this;
        }

        public FileStorageBuilder AddConfiguration<TConfig>(Type storageType, IConfiguration configuration) where TConfig : class
        {
            var type = typeof(TConfig);

            var cache = new ConfigurationCache(storageType, Types, type, configuration);

            if (!ConfigurationCache.TryAdd(type, cache))
                throw new ArgumentException($"configuration for {storageType} already exist");

            return this;
        }
    }

    public interface IFileStorageBuilder
    {
        public IServiceCollection Services { get; set; }

        public IDictionary<string, PropertyInfo[]> Properties { get; }
        public IList<Type> Types { get; }
        public IDictionary<Type, ConfigurationCache> ConfigurationCache { get; }

        FileStorageBuilder AddFile<T>() where T : class, new();
        FileStorageBuilder AddConfiguration<TConfig>(Type storageType, IConfiguration configuration) where TConfig : class;

    }
}