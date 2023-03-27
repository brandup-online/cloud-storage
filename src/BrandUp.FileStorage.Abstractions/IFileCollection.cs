namespace BrandUp.FileStorage
{
    public interface IFileCollection<TMetadata>
        where TMetadata : class, new()
    {
        string Name { get; }
        Task<File<TMetadata>> UploadFileAsync(Guid fileId, TMetadata file, Stream fileStream, CancellationToken cancellationToken = default);
        Task<File<TMetadata>> FindFileAsync(Guid fileId, CancellationToken cancellationToken = default);
        Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default);
    }
}