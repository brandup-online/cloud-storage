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
        public IDictionary<Type, PropertyCacheCollection> Properties { get; init; }
        IDictionary<Type, ConfigurationCache> IFileStorageBuilder.ConfigurationCache => configurationCache; // Type is config type

        /// <summary>
        /// Constructor of builder
        /// </summary>
        /// <param name="services"> Service collection </param>
        /// <exception cref="ArgumentNullException"> throws if services is null </exception>
        public FileStorageBuilder(IServiceCollection services)
        {
            Properties = new Dictionary<Type, PropertyCacheCollection>();
            Services = services ?? throw new ArgumentNullException(nameof(services));

            configurationCache = new Dictionary<Type, ConfigurationCache>();

            Services.AddScoped<IFileStorageFactory, FileStorageFactory>();
            Services.AddSingleton<IFileStorageBuilder, FileStorageBuilder>(f =>
            {
                return this;
            });
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
            var type = typeof(TFile);
            var configType = configuration.GetType();

            if (configurationCache.TryGetValue(configType, out var cache))
                cache.Add(type, configuration);
            else throw new ArgumentException("Unknown configuration. First you need add this configuration to builder");

            var propertyCollection = new PropertyCacheCollection();
            var properties = type.GetProperties();
            GeneratePropertyCollection(propertyCollection, properties);

            if (!Properties.TryGetValue(type, out _))
                Properties.Add(type, propertyCollection);

            return this;
        }

        #endregion

        #region Helpers 
        void GeneratePropertyCollection(PropertyCacheCollection collection, PropertyInfo[] properties, string name = default)
        {
            foreach (var prop in properties)
            {
                if (!prop.PropertyType.IsSerializable)
                {
                    if (name == null)
                        GeneratePropertyCollection(collection, prop.PropertyType.GetProperties(), prop.Name);
                    else
                        GeneratePropertyCollection(collection, prop.PropertyType.GetProperties(), name + "." + prop.Name);
                }
                else
                {
                    if (name != null)
                        collection.Add(name + "." + prop.Name, prop);
                    else
                        collection.Add(prop.Name, prop);
                }
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