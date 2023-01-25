using BrandUp.FileStorage.Exceptions;

namespace BrandUp.FileStorage
{
    /// <summary>
    /// Interface for work with the storage.
    /// </summary>
    /// <typeparam name="TMetadata">Define file for work</typeparam>
    public interface IFileStorage<TMetadata> : IDisposable where TMetadata : class, IFileMetadata, new()
    {
        /// <summary>
        /// Uploads file to the store
        /// </summary>
        /// <param name="fileInfo">Metadata for save</param>
        /// <param name="fileStream">Stream of saving file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Information of file with metadata</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="AccessDeniedException"></exception>
        /// <exception cref="IntegrationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        Task<FileInfo<TMetadata>> UploadFileAsync(TMetadata fileInfo, Stream fileStream, CancellationToken cancellationToken = default);
        /// <summary>
        /// Uploads file to the store with predefined id
        /// </summary>
        /// <param name="fileId">Id of file in storage </param>
        /// <param name="fileInfo">Metadata for save</param>
        /// <param name="fileStream">Stream of saving file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Information of file with metadata</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="AccessDeniedException"></exception>
        /// <exception cref="IntegrationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        Task<FileInfo<TMetadata>> UploadFileAsync(Guid fileId, TMetadata fileInfo, Stream fileStream, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets metadata of file
        /// </summary>
        /// <param name="fileId">Id of file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Information of file with metadata</returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="AccessDeniedException"></exception>
        /// <exception cref="IntegrationException"></exception>
        Task<FileInfo<TMetadata>> GetFileInfoAsync(Guid fileId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reads file from storage
        /// </summary>
        /// <param name="fileId">Id of file</param>
        /// <param name="cancellationToken">Cancellation tokSen</param>
        /// <returns>File stream</returns>
        /// <exception cref="NotFoundException">If file does not exist in storage</exception>
        /// <exception cref="AccessDeniedException">If user have not permisions to read file</exception>
        /// <exception cref="IntegrationException">Other storage exeptions</exception>
        Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deletes file from storage
        /// </summary>
        /// <param name="fileId">Id of file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>true - if file deletes, false - if not</returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="IntegrationException"></exception>
        Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default);
    }
}