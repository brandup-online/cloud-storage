using BrandUp.FileStorage.Internals.Context;

namespace BrandUp.FileStorage
{
    /// <summary>
    /// Базовый класс контекста, который содержит определения коллекций файлов и обеспечивает доступ к ним.
    /// </summary>
    public abstract class FileStorageContext
    {
        readonly Dictionary<string, FileCollectionImpl> collections = new();
        readonly Dictionary<string, string> bucketsDictionary;
        IStorageProvider storageProvider;
        StorageContextInfo contextInfo;

        public IStorageProvider StorageProvider => storageProvider;

        internal void Initialize(IStorageProvider storageProvider, StorageContextInfo contextInfo)
        {
            this.storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            this.contextInfo = contextInfo ?? throw new ArgumentNullException(nameof(contextInfo));
        }

        public IFileCollection<TMetadata> GetCollection<TMetadata>(string collectionName)
            where TMetadata : class, new()
        {
            if (collectionName == null)
                throw new ArgumentNullException(nameof(collectionName));

            var normalizedCollectionName = collectionName.Trim().ToLower();

            if (collections.TryGetValue(normalizedCollectionName, out var fileCollection))
                return (IFileCollection<TMetadata>)fileCollection;

            var metadataProvider = contextInfo.GetMetadataProvider<TMetadata>();

            var collection = new FileCollectionImpl<TMetadata>(collectionName, storageProvider, metadataProvider);
            collections.Add(normalizedCollectionName, collection);

            return collection;
        }
    }
}