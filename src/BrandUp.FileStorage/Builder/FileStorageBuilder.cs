using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Abstract.Configuration;
using BrandUp.FileStorage.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Builder
{
    /// <summary>
    /// Implementation of IFileStorageBuilder
    /// </summary>
    public class FileStorageBuilder : IFileStorageBuilder, IFileDefinitionsDictionary
    {
        readonly IDictionary<Type, IFileStorageConfiguration> configurations; // Type is IFileStorage Type
        readonly IDictionary<Type, FileMetadataDefinition> properties;// Type is IFileMetadata File

        public FileStorageBuilder(IServiceCollection services)
        {
            properties = new Dictionary<Type, FileMetadataDefinition>();
            configurations = new Dictionary<Type, IFileStorageConfiguration>();

            Services = services ?? throw new ArgumentNullException(nameof(services));

            Services.AddScoped<IFileStorageFactory, FileStorageFactory>();
            Services.AddSingleton<IFileDefinitionsDictionary>(f => this);
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
        public IFileStorageBuilder AddStorage(Type storageType, IFileStorageConfiguration configuration)
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
        /// Adds file type with it key to builder
        /// </summary>
        /// <typeparam name="TFile">file type to add</typeparam>
        /// <param name="storageType">Type of storage for file</param>
        /// <param name="configurationKey">Configuration for this file</param>
        /// <returns>Same instance of builder</returns>
        /// <exception cref="ArgumentException">Throws if type of configuration not added to builder yet</exception>
        public IFileStorageBuilder AddFileToStorage<TFile>(Type storageType, string configurationKey) where TFile : class, new()
        {
            if (storageType == null)
                throw new ArgumentNullException(nameof(storageType));
            if (!storageType.IsAssignableToGenericType(typeof(IFileStorage<>)))
                throw new ArgumentException($"{nameof(storageType)} must be assignable to {typeof(IFileStorage<>)}");
            if (configurationKey == null)
                throw new ArgumentNullException(nameof(configurationKey));

            var fileType = typeof(TFile);
            if (properties.ContainsKey(fileType))
                throw new InvalidOperationException($"File {fileType.Name} for {storageType.Name} already exist");

            if (!configurations.TryGetValue(storageType, out var storageConfiguration))
                throw new Exception($"Builder does not contain configuration for {storageType.Name}");

            var fileTypeDefinition = new FileMetadataDefinition(fileType, storageType);
            properties.Add(fileTypeDefinition.MetadataFileType, fileTypeDefinition);

            var defaultConfigKey = fileType.Name; // By default file name is key for file configuration
            if (configurationKey != string.Empty)
                defaultConfigKey = configurationKey;

            if (storageConfiguration.InnerConfiguration.TryGetValue(defaultConfigKey, out var fileConfig))
            {
                fileTypeDefinition.AddConfiguration(storageConfiguration, fileConfig);
            }
            else
                fileTypeDefinition.AddConfiguration(storageConfiguration, null);
            return this;
        }

        /// <summary>
        /// Adds file type with it configuration to builder
        /// </summary>
        /// <typeparam name="TFile">file type to add</typeparam>
        /// <param name="storageType">Type of storage for file</param>
        /// <param name="configuration">Configuration for file</param>
        /// <returns>Same instance of builder</returns>
        /// <exception cref="ArgumentException">Throws if type of configuration not added to builder yet</exception>
        public IFileStorageBuilder AddFileToStorage<TFile>(Type storageType, IFileMetadataConfiguration configuration) where TFile : class, new()
        {
            if (storageType == null)
                throw new ArgumentNullException(nameof(storageType));
            if (!storageType.IsAssignableToGenericType(typeof(IFileStorage<>)))
                throw new ArgumentException($"{nameof(storageType)} must be assignable to {typeof(IFileStorage<>)}");
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var fileType = typeof(TFile);
            if (properties.ContainsKey(fileType))
                throw new InvalidOperationException();

            var fileTypeDefinition = new FileMetadataDefinition(fileType, storageType);
            properties.Add(fileTypeDefinition.MetadataFileType, fileTypeDefinition);

            if (!configurations.TryGetValue(storageType, out var storageConfiguration))
                throw new Exception($"Builder does not contain configuration for {storageType.Name}");

            fileTypeDefinition.AddConfiguration(storageConfiguration, configuration);

            return this;
        }

        #endregion

        #region IFileDefinitionsDictionary members

        public bool TryGetConstructor(Type type, out IStorageInstanceCreator value)
        {
            if (properties.TryGetValue(type, out var property))
                if (property is FileMetadataDefinition metadataDefinition)
                {
                    value = metadataDefinition;
                    return true;
                }

            value = null;
            return false;
        }

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