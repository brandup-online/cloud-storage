using BrandUp.FileStorage.Internals.Context;

namespace BrandUp.FileStorage
{
    /// <summary>
    /// Базовый класс контекста, который содержит определения коллекций файлов и обеспечивает доступ к ним.
    /// </summary>
    public abstract class FileStorageContext
    {
        readonly Dictionary<string, FileCollectionImpl> collections = new();
        IStorageProvider storageProvider;
        StorageContextInfo contextInfo;
        ContextConfiguration contextConfiguration;

        public IStorageProvider StorageProvider => storageProvider;

        internal void Initialize(IStorageProvider storageProvider, StorageContextInfo contextInfo, ContextConfiguration contextConfiguration)
        {
            this.storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            this.contextInfo = contextInfo ?? throw new ArgumentNullException(nameof(contextInfo));
            this.contextConfiguration = contextConfiguration ?? throw new ArgumentNullException(nameof(contextConfiguration));
        }

        public IFileCollection<TMetadata> GetCollection<TMetadata>(string collectionKey)
            where TMetadata : class, new()
        {
            if (collectionKey == null)
                throw new ArgumentNullException(nameof(collectionKey));

            if (!contextConfiguration.TryGetConfiguration(collectionKey, out var collectionName))
                collectionName = collectionKey;

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