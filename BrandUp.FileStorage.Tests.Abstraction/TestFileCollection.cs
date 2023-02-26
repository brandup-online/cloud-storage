namespace BrandUp.FileStorage
{
    internal class TestFileCollection<TMetadata> : IFileCollection<TMetadata> where TMetadata : class, new()
    {
        public string Name => throw new NotImplementedException();

        public Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<File<TMetadata>> FindFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<File<TMetadata>> UploadFileAsync(Guid fileId, TMetadata file, Stream fileStream, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
