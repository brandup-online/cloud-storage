namespace BrandUp.FileStorage.Internals.Context
{
    internal class FileCollectionImpl<TMetadata> : FileCollectionImpl, IFileCollection<TMetadata>
        where TMetadata : class, new()
    {
        readonly string collectionName;
        readonly IStorageProvider storageProvider;
        readonly FileMetadataProvider metadataProvider;

        public FileCollectionImpl(string collectionName, IStorageProvider storageProvider, FileMetadataProvider metadataProvider)
        {
            this.collectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
            this.storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            this.metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
        }

        #region IFileCollection members

        public override string Name => collectionName;

        public async Task<File<TMetadata>> UploadFileAsync(Guid fileId, TMetadata metadata, Stream fileStream, CancellationToken cancellationToken = default)
        {
            if (fileId == Guid.Empty)
                throw new ArgumentException();
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            var data = new Dictionary<string, string>();
            metadataProvider.Serialize(metadata, data);

            var fileInfo = await storageProvider.UploadFileAsync(collectionName, fileId, data, fileStream, cancellationToken);

            return new File<TMetadata>(fileInfo, metadata);
        }

        public async Task<File<TMetadata>> FindFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            var fileInfo = await storageProvider.FindFileAsync(collectionName, fileId, cancellationToken);

            var metadata = metadataProvider.Deserialize<TMetadata>(fileInfo.Metadata);

            return new File<TMetadata>(fileInfo, metadata);
        }

        public Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return storageProvider.ReadFileAsync(collectionName, fileId, cancellationToken);
        }

        public Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            return storageProvider.DeleteFileAsync(collectionName, fileId, cancellationToken);
        }

        #endregion
    }

    internal abstract class FileCollectionImpl
    {
        public abstract string Name { get; }
    }
}