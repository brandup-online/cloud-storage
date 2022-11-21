using CloudStorage.Models;
using CloudStorage.Models.Interfaces;

namespace CloudStorage.Client
{
    public interface ICloudClient<TMetadata> : IDisposable where TMetadata : class, IFileMetadata, new()
    {

        public Task<FileInfo<TMetadata>> UploadFileAsync(TMetadata fileInfo, Stream fileStream, CancellationToken cancellationToken = default);
        public Task<FileInfo<TMetadata>> UploadFileAsync(Guid fileId, TMetadata fileInfo, Stream fileStream, CancellationToken cancellationToken = default);
        Task<FileInfo<TMetadata>> GetFileInfoAsync(Guid fileId, CancellationToken cancellationToken = default);
        Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default);
    }
}
