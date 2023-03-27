namespace BrandUp.FileStorage
{
    public interface IStorageProvider
    {
        Task<FileInfo> UploadFileAsync(string bucketName, Guid fileId, Dictionary<string, string> metadata, Stream fileStream, CancellationToken cancellationToken);
        Task<FileInfo> FindFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken);
        Task<Stream> ReadFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken);
        Task<bool> DeleteFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken = default);
    }

    public class FileInfo
    {
        public Guid Id { get; }
        public long Size { get; }
        public Dictionary<string, string> Metadata { get; }

        public FileInfo(Guid id, long size, Dictionary<string, string> metadata)
        {
            Id = id;
            Size = size;
            Metadata = metadata;
        }
    }
}