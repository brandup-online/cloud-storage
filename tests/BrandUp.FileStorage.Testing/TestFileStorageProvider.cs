using System.Collections.Concurrent;

namespace BrandUp.FileStorage.Testing
{
    public class TestStorageProvider : IStorageProvider
    {
        readonly ConcurrentDictionary<string, Bucket> buckets = new();
        readonly TestStorageOptions options;

        public TestStorageProvider(TestStorageOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        #region IStorageProvider members

        public async Task<FileInfo> UploadFileAsync(string bucketName, Guid fileId, Dictionary<string, string> metadata, Stream fileStream, CancellationToken cancellationToken)
        {
            var bucket = GetBucket(bucketName);

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream, cancellationToken);

            var file = new MemoryFile(fileId, metadata, memoryStream.ToArray());
            bucket.Add(file);

            return new FileInfo(file.Id, file.Data.Length, file.Metadata);
        }

        public Task<FileInfo> FindFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken)
        {
            var bucket = GetBucket(bucketName);

            bucket.TryGet(fileId, out var file);

            return Task.FromResult(file != null ? new FileInfo(file.Id, file.Data.Length, file.Metadata) : null);
        }

        public Task<Stream> ReadFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken)
        {
            var bucket = GetBucket(bucketName);

            if (!bucket.TryGet(fileId, out var file))
                return Task.FromResult<Stream>(null);

            return Task.FromResult<Stream>(new MemoryStream(file.Data));
        }

        public Task<bool> DeleteFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken = default)
        {
            var bucket = GetBucket(bucketName);

            return Task.FromResult(bucket.Delete(fileId));
        }

        #endregion

        #region Helpers

        Bucket GetBucket(string name)
        {
            return buckets.GetOrAdd(name, n =>
            {
                return new Bucket(n);
            });
        }

        #endregion

        class Bucket
        {
            readonly ConcurrentDictionary<Guid, MemoryFile> files = new();

            public string Name { get; }
            public int Count => files.Count;

            public Bucket(string name)
            {
                Name = name;
            }

            public void Add(MemoryFile file)
            {
                files.AddOrUpdate(file.Id, file, (id, cur) => file);
            }

            public bool TryGet(Guid id, out MemoryFile file)
            {
                return files.TryGetValue(id, out file);
            }

            public bool Delete(Guid id)
            {
                return files.Remove(id, out _);
            }
        }

        class MemoryFile
        {
            public Guid Id { get; }
            public Dictionary<string, string> Metadata { get; }
            public byte[] Data { get; }

            public MemoryFile(Guid id, Dictionary<string, string> metadata, byte[] data)
            {
                Id = id;
                Metadata = metadata;
                Data = data;
            }
        }
    }

    public class TestStorageOptions
    {
        public int MaxFilesByBucket { get; set; } = 0;
        public int MaxFileSize { get; set; } = 0;
    }
}