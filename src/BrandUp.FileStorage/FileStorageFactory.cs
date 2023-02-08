using BrandUp.FileStorage.Abstract;

namespace BrandUp.FileStorage
{
    /// <summary>
    /// Factory class for IFileStorage
    /// </summary>
    public class FileStorageFactory : IFileStorageFactory
    {
        readonly IDictionary<Type, object> cachedStorages;
        readonly IFileDefinitionsDictionary fileDefinitions;
        readonly IServiceProvider provider;

        public FileStorageFactory(IStorageInstanceCreator cache, IFileDefinitionsDictionary fileDefinitions)
        {
            this.fileDefinitions = fileDefinitions ?? throw new ArgumentNullException(nameof(fileDefinitions));
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            cachedStorages = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Method creates instanse of storage by config type.
        /// </summary>
        /// <typeparam name="TFileType">File for that creates storage</typeparam>
        /// <returns>instance of IFileStorage</returns>
        public IFileStorage<TFileType> Create<TFileType>() where TFileType : class, IFileMetadata, new()
        {
            if (!fileDefinitions.TryGetConstructor(typeof(TFileType), out var value))

            {
                var storage = value.CreateStorageInstance<TFileType>(provider);
                if (storage is IFileStorage<TFileType> typedStorage)
                {
                    cachedStorages.Add(typeof(TFileType), typedStorage);
                    return typedStorage;
                }
                else
                {
                    throw new InvalidOperationException("Unknown Type.");
                }
            }
            else return value as IFileStorage<TFileType>;
        }
    }
}