using BrandUp.FileStorage.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BrandUp.FileStorage.Builder
{
    /// <summary>
    /// Implementation of IFileStorageBuilder
    /// </summary>
    public class FileStorageBuilder : IFileStorageBuilder
    {
        readonly IDictionary<Type, ConfigurationCache> configurationCache;

        /// <summary>
        /// Dictionary where key is type of metadata property and value is collection of properties of this type 
        /// </summary>
        public IDictionary<Type, FileMetadataDefinition> Properties { get; init; }
        IDictionary<Type, ConfigurationCache> IFileStorageBuilder.ConfigurationCache => configurationCache; // Type is config type

        public FileStorageBuilder(IServiceCollection services)
        {
            Properties = new Dictionary<Type, FileMetadataDefinition>();
            Services = services ?? throw new ArgumentNullException(nameof(services));

            configurationCache = new Dictionary<Type, ConfigurationCache>();

            Services.AddScoped<IFileStorageFactory, FileStorageFactory>();
            Services.AddSingleton<IFileStorageBuilder, FileStorageBuilder>(f => this);
        }

        #region IFileStorageBuilder members

        /// <summary>
        /// Serivice collection
        /// </summary>
        public IServiceCollection Services { get; set; }

        /// <summary>
        /// Adds a new cofiguration for client 
        /// </summary>
        /// <typeparam name="TConfig">Configuration type</typeparam>
        /// <param name="storageType">Type of storage for which added configuration</param>
        /// <param name="configuration">Configuration object</param>
        /// <returns>Same instance of builder</returns>
        /// <exception cref="ArgumentException"></exception>
        public FileStorageBuilder AddConfiguration<TConfig>(Type storageType, TConfig configuration) where TConfig : class
        {
            var configType = typeof(TConfig);

            var cache = new ConfigurationCache(storageType, configType, configuration);

            if (!configurationCache.TryAdd(configType, cache))
                throw new ArgumentException($"Configuration for {storageType} already exist");

            return this;
        }

        /// <summary>
        /// Adds file type with it configuration to builder
        /// </summary>
        /// <typeparam name="TFile">file type to add</typeparam>
        /// <param name="configuration">Configuration for this file</param>
        /// <returns>Same instance of builder</returns>
        /// <exception cref="ArgumentException">Throws if type of configuration not added to builder yet</exception>
        public FileStorageBuilder AddFileToStorage<TFile>(object configuration) where TFile : class, new()
        {
            var fileType = typeof(TFile);
            if (Properties.ContainsKey(fileType))
                throw new InvalidOperationException();

            var configType = configuration.GetType();

            if (configurationCache.TryGetValue(configType, out var cache))
                cache.Add(fileType, configuration);
            else
                throw new ArgumentException("Unknown configuration. First you need add this configuration to builder");

            var fileTypeDefinition = new FileMetadataDefinition(fileType);
            Properties.Add(fileTypeDefinition.MetadataFileType, fileTypeDefinition);

            return this;
        }

        #endregion

        #region Helpers 

        void GeneratePropertyCollection(PropertyCacheCollection collection, Type type, string parentName = default)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
            foreach (var property in properties)
            {
                var propertyName = property.GetCustomAttribute<MetadataKeyAttribute>()?.Name ?? property.Name;
                var metadataName = parentName == null ? propertyName : parentName + "." + propertyName;

                if (!property.PropertyType.IsSerializable)
                    GeneratePropertyCollection(collection, property.PropertyType, metadataName);
                else
                    collection.AddProperty(metadataName, property);
            }
        }

        #endregion
    }

    /// <summary>
    /// Storage constructor. Storage and configurations for it are added here
    /// </summary>
    public interface IFileStorageBuilder
    {
        /// <summary>
        /// Service collection property
        /// </summary>
        IServiceCollection Services { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IDictionary<Type, PropertyCacheCollection> Properties { get; } // Type is file type

        internal IDictionary<Type, ConfigurationCache> ConfigurationCache { get; }

        /// <summary>
        /// Adds a new cofiguration for client 
        /// </summary>
        /// <typeparam name="TConfig">Configuration type</typeparam>
        /// <param name="storageType">Type of storage for which added configuration</param>
        /// <param name="configuration">Configuration object</param>
        /// <returns>Same instance of builder</returns>
        FileStorageBuilder AddConfiguration<TConfig>(Type storageType, TConfig configuration) where TConfig : class;

        /// <summary>
        /// Adds file type with it configuration to builder
        /// </summary>
        /// <typeparam name="TFile">file type to add</typeparam>
        /// <param name="configuration">Configuration for this file</param>
        /// <returns>Same instance of builder</returns>
        FileStorageBuilder AddFileToStorage<TFile>(object configuration) where TFile : class, new();
    }
}