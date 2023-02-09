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

        public FileStorageFactory(IServiceProvider provider, IFileDefinitionsDictionary fileDefinitions)
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
            var fileType = typeof(TFileType);
            if (!cachedStorages.TryGetValue(fileType, out var storageInstance))
            {
                if (fileDefinitions.TryGetConstructor(fileType, out var constructor))
                {
                    var storage = constructor.CreateStorageInstance<TFileType>(provider);
                    if (storage is IFileStorage<TFileType> typedStorage)
                    {
                        cachedStorages.Add(typeof(TFileType), typedStorage);
                        return typedStorage;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown type of storage.");
                    }
                }
                else throw new InvalidOperationException($"File {fileType.Name} not contains in IFileDefinitionsDictionary.");
            }
            else return storageInstance as IFileStorage<TFileType>;
        }
    }
}