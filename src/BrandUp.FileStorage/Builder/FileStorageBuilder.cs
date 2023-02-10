using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Abstract.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrandUp.FileStorage.Builder
{
    /// <summary>
    /// Implementation of IFileStorageBuilder
    /// </summary>
    public class FileStorageBuilder : IFileStorageBuilder, IFileDefinitionsContext
    {
        readonly IDictionary<Type, IDictionary<string, IStorageConfiguration>> configurations; // Type is IFileStorage type
        readonly IDictionary<Type, FileMetadataDefinition> properties;// Type is IFileMetadata type

        const string dafaultConfigurationKey = "Default";

        public FileStorageBuilder(IServiceCollection services)
        {
            properties = new Dictionary<Type, FileMetadataDefinition>();
            configurations = new Dictionary<Type, IDictionary<string, IStorageConfiguration>>();

            Services = services ?? throw new ArgumentNullException(nameof(services));

            Services.AddSingleton<IFileDefinitionsContext>(f => this);
        }

        #region IFileStorageBuilder members

        /// <summary>
        /// Serivice collection
        /// </summary>
        public IServiceCollection Services { get; set; }

        /// <summary>
        /// Adds a new cofiguration for client 
        /// </summary>
        /// <param name="storageType">Type of storage for which added configuration</param>
        /// <param name="configuration">Configuration object</param>
        /// <returns>Same instance of builder</returns>
        /// <exception cref="ArgumentException"></exception>
        public IFileStorageBuilder AddStorage(Type storageType, IDictionary<string, IStorageConfiguration> configuration)
        {
            if (storageType == null)
                throw new ArgumentNullException(nameof(storageType));
            if (!storageType.IsAssignableToGenericType(typeof(IFileStorage<>)))
                throw new ArgumentException($"{nameof(storageType)} must be assignable to {typeof(IFileStorage<>)}");
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (!configurations.TryAdd(storageType, configuration))
                throw new ArgumentException($"Configuration for {storageType.Name} already exist");

            return this;
        }

        /// <summary>
        /// Adds file type with it configuration to builder
        /// </summary>
        /// <typeparam name="TFile">file type to add</typeparam>
        /// <param name="storageType">Type of storage for file</param>
        /// <param name="configuration">Configuration for file</param>
        /// <param name="configurationKey"></param>
        /// <returns>Same instance of builder</returns>
        /// <exception cref="ArgumentException">Throws if type of configuration not added to builder yet</exception>
        public IFileStorageBuilder AddFileToStorage<TFile>(Type storageType, IStorageConfiguration configuration, string configurationKey = "") where TFile : class, IFileMetadata, new()
        {
            if (storageType == null)
                throw new ArgumentNullException(nameof(storageType));
            if (!storageType.IsAssignableToGenericType(typeof(IFileStorage<>)))
                throw new ArgumentException($"{nameof(storageType)} must be assignable to {typeof(IFileStorage<>).Name}");

            var fileType = typeof(TFile);
            if (properties.ContainsKey(fileType))
                throw new InvalidOperationException($"File {fileType.Name} for {storageType.Name} already exist");

            if (!configurations.TryGetValue(storageType, out var storageConfiguration))
                throw new Exception($"Builder does not contain configuration for {storageType.Name}");

            if (!storageConfiguration.TryGetValue(dafaultConfigurationKey, out var defaultConfiguration))
                throw new Exception($"Builder does not contain default configuration for {storageType.Name}");

            var fileTypeDefinition = new FileMetadataDefinition(fileType, storageType);
            properties.Add(fileTypeDefinition.MetadataFileType, fileTypeDefinition);

            var fileConfigKey = fileType.Name; // By default file name is key for file configuration
            if (configurationKey != string.Empty)
                fileConfigKey = configurationKey;

            if (!storageConfiguration.TryGetValue(fileConfigKey, out var fileConfig))
            {
                storageConfiguration.Add(fileConfigKey, configuration);
                fileConfig = configuration;
            }

            fileTypeDefinition.AddConfiguration(defaultConfiguration, fileConfig);
            Services.AddScoped(fileTypeDefinition.CreateStorageInstance<TFile>);

            return this;
        }

        #endregion

        #region IFileDefinitionsDictionary members

        public bool TryGetProperties(Type type, out IEnumerable<IPropertyCache> value)
        {
            if (properties.TryGetValue(type, out var property))
                if (property is IEnumerable<IPropertyCache> metadataDefinition)
                {
                    value = metadataDefinition;
                    return true;
                }

            value = null;
            return false;
        }

        #endregion
    }
}