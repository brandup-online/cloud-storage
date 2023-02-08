using BrandUp.FileStorage.Builder;

namespace BrandUp.FileStorage
{
    /// <summary>
    /// Factory class for IFileStorage
    /// </summary>
    public class FileStorageFactory : IFileStorageFactory
    {
        readonly IFileStorageBuilder builder;
        readonly IServiceProvider provider;

        public FileStorageFactory(IFileStorageBuilder builder, IServiceProvider provider)
        {
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Method creates instanse of storage by config type.
        /// </summary>
        /// <typeparam name="TFileType">File for that creates storage</typeparam>
        /// <typeparam name="TConfigType">Configuration is defines type of storage</typeparam>
        /// <returns>instance of IFileStorage</returns>
        public IFileStorage<TFileType> Create<TFileType, TConfigType>() where TFileType : class, IFileMetadata, new()
                                                                        where TConfigType : class, new()
           => builder.ConfigurationCache[typeof(TConfigType)].CreateInstanse<TFileType>(provider);
    }

    public interface IFileStorageDefinitions
    {
        bool TryGet(Type fileType, out FileMetadataDefinition fileDefinition);
    }

    /// <summary>
    /// Factory class for IFileStorage
    /// </summary>
    public interface IFileStorageFactory
    {
        /// <summary>
        /// Method creates instanse of storage by config type
        /// </summary>
        /// <typeparam name="TFileType">File for that creates storage</typeparam>
        /// <typeparam name="TConfigType">Configuration is defines type of storage</typeparam>
        /// <returns>instance of IFileStorage</returns>
        public IFileStorage<TFileType> Create<TFileType, TConfigType>() where TFileType : class, IFileMetadata, new()
            where TConfigType : class, new();
    }
}