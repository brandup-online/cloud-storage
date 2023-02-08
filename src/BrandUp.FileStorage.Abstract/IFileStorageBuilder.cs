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
        /// <param name="configuration">Configuration object</param>
        /// <returns>Same instance of builder</returns>
        IFileStorageBuilder AddStorage(Type storageType, object configuration);

        /// <summary>
        /// Adds file type with it configuration to builder
        /// </summary>
        /// <typeparam name="TFile">file type to add</typeparam>
        /// <param name="configuration">Configuration for this file</param>
        /// <returns>Same instance of builder</returns>
        IFileStorageBuilder AddFileToStorage<TFile>(Type storageType, object configuration) where TFile : class, new();
    }
}
