using BrandUp.FileStorage.Abstract.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Abstract
{

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
        /// Adds a new cofiguration for client 
        /// </summary>
        /// <param name="storageType">Type of storage for which added configuration</param>
        /// <param name="configuration">Storage Configuration</param>
        /// <returns>Same instance of builder</returns>
        IFileStorageBuilder AddStorage(Type storageType, IDictionary<string, IStorageConfiguration> configuration);

        /// <summary>
        /// Adds file type with it key to builder
        /// </summary>
        /// <typeparam name="TFile">file type to add</typeparam>
        /// <param name="configuration">Configuration for this file</param>
        /// <returns>Same instance of builder</returns>
        IFileStorageBuilder AddFileToStorage<TFile>(Type storageType, IStorageConfiguration configuration, string configurationKey = "") where TFile : class, IFileMetadata, new();
    }
}
