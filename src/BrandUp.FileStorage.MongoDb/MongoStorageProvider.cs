using BrandUp.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace BrandUp.FileStorage.MongoDb
{
    public class MongoStorageProvider<TMongoContext> : IStorageProvider
        where TMongoContext : MongoDbContext
    {
        readonly Dictionary<string, FileBucket> buckets = new();
        readonly TMongoContext documentContext;

        public MongoStorageProvider(TMongoContext documentContext)
        {
            this.documentContext = documentContext ?? throw new ArgumentNullException(nameof(documentContext));
        }

        #region IStorageProvider members

        public async Task<FileInfo> UploadFileAsync(string bucketName, Guid fileId, Dictionary<string, string> metadata, Stream fileStream, CancellationToken cancellationToken)
        {
            var bsonProperties = new List<BsonElement>();
            foreach (var kv in metadata)
                bsonProperties.Add(new BsonElement(kv.Key, BsonValue.Create(kv.Value)));

            var fileKey = fileId.ToString().ToLower();
            var uploadOptions = new GridFSUploadOptions
            {
                DisableMD5 = false,
                Metadata = new BsonDocument(bsonProperties)
            };

            var bucket = GetBucket(bucketName);

            await bucket.UploadFromStreamAsync(fileKey, fileKey, fileStream, uploadOptions, cancellationToken);

            return await FindFileAsync(bucketName, fileId, cancellationToken);
        }

        public async Task<FileInfo> FindFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken)
        {
            var fileKey = fileId.ToString().ToLower();
            var bucket = GetBucket(bucketName);

            var filter = Builders<GridFSFileInfo<string>>.Filter.Eq(info => info.Id, fileKey);
            var cursor = await bucket.FindAsync(filter, cancellationToken: cancellationToken);

            var fileInfo = await cursor.SingleOrDefaultAsync(cancellationToken);
            if (fileInfo == null)
                return null;

            var metadata = new Dictionary<string, string>();
            foreach (var elem in fileInfo.Metadata.Elements)
                metadata.Add(elem.Name, elem.Value.AsString);

            return new FileInfo(fileId, fileInfo.Length, metadata);
        }

        public async Task<Stream> ReadFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken)
        {
            var fileKey = fileId.ToString().ToLower();
            var bucket = GetBucket(bucketName);

            try
            {
                return await bucket.OpenDownloadStreamAsync(fileKey, cancellationToken: cancellationToken);
            }
            catch (GridFSFileNotFoundException)
            {
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken = default)
        {
            var fileKey = fileId.ToString().ToLower();
            var bucket = GetBucket(bucketName);

            await bucket.DeleteAsync(fileKey, cancellationToken);
            return true;
        }

        #endregion

        #region Helpers

        FileBucket GetBucket(string bucketName)
        {
            if (buckets.TryGetValue(bucketName, out var bucket))
                return bucket;

            bucket = new FileBucket(documentContext.Database, new GridFSBucketOptions { BucketName = bucketName, DisableMD5 = false });
            buckets.Add(bucketName, bucket);

            return bucket;
        }

        #endregion

        class FileBucket : GridFSBucket<string>
        {
            public FileBucket(IMongoDatabase database, GridFSBucketOptions options = null) : base(database, options) { }
        }
    }

    public class MongoStorageOptions { }
}