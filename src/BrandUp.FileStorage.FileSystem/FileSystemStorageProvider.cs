using BrandUp.FileStorage.Exceptions;
using BrandUp.FileStorage.FileSystem.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace BrandUp.FileStorage.FileSystem
{
    /// <summary>
    /// Storage for local file system
    /// </summary>
    public class FileSystemStorageProvider : IStorageProvider
    {
        readonly FolderConfiguration folderConfiguration;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderConfiguration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSystemStorageProvider(FolderConfiguration folderConfiguration)
        {
            this.folderConfiguration = folderConfiguration ?? throw new ArgumentNullException(nameof(folderConfiguration));

            if (!Directory.Exists(this.folderConfiguration.ContentPath))
                Directory.CreateDirectory(this.folderConfiguration.ContentPath);

            if (!Directory.Exists(this.folderConfiguration.MetadataPath))
                Directory.CreateDirectory(this.folderConfiguration.MetadataPath);
        }

        #region IStorageProvider members

        /// <summary>
        /// Uploads file to the store with predefined id
        /// </summary>
        /// <param name="bucketName">Id of file in storage </param>
        /// <param name="fileId">Id of file in storage </param>
        /// <param name="metadata">Metadata for save</param>
        /// <param name="fileStream">Stream of saving file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Information of file with metadata</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="AccessDeniedException"></exception>
        /// <exception cref="IntegrationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task<FileInfo> UploadFileAsync(string bucketName, Guid fileId, Dictionary<string, string> metadata, Stream fileStream, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString());
            var metadataPath = Path.Combine(folderConfiguration.MetadataPath, fileId.ToString() + ".json");

            if (fileStream.Length == 0)
                throw new InvalidOperationException("File does not contain any data");

            try
            {
                if (!File.Exists(filePath))
                {
                    using var file = File.Create(filePath);
                    fileStream.CopyTo(file);
                }
                else throw new ArgumentException($"File with key {fileId} already exist");

                if (!File.Exists(metadataPath))
                {
                    using var file = File.Create(metadataPath);
                    var json = JsonConvert.SerializeObject(metadata);
                    await file.WriteAsync(Encoding.ASCII.GetBytes(json), cancellationToken);
                }
                else throw new ArgumentException($"Metadata file with key {fileId} already exist");
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new AccessDeniedException(ex);
            }
            catch (Exception ex)
            {
                throw new IntegrationException(ex);
            }


            return new FileInfo(fileId, fileStream.Length, metadata)
            {
            };
        }

        /// <summary>
        /// Gets metadata of file
        /// </summary>
        /// <param name="fileId">Id of file</param>
        /// <param name="bucketName">Id of file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Information of file with metadata</returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="AccessDeniedException"></exception>
        /// <exception cref="IntegrationException"></exception>
        public async Task<FileInfo> FindFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken = default)
        {
            var metadataPath = Path.Combine(folderConfiguration.MetadataPath, fileId.ToString() + ".json");

            if (File.Exists(metadataPath))
            {
                try
                {
                    using var metadataFile = File.OpenRead(metadataPath);
                    using var reader = new StreamReader(metadataFile);
                    var json = await reader.ReadToEndAsync();

                    var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString());
                    var metadata = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                    return new FileInfo(fileId, new System.IO.FileInfo(filePath).Length, metadata);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new AccessDeniedException(ex);
                }
                catch (Exception ex)
                {
                    throw new IntegrationException(ex);
                }
            }
            else return null;
        }

        /// <summary>
        /// Reads file from storage
        /// </summary>
        /// <param name="bucketName">Id of file</param>
        /// <param name="fileId">Id of file</param>
        /// <param name="cancellationToken">Cancellation tokSen</param>
        /// <returns>File stream</returns>
        /// <exception cref="NotFoundException">If file does not exist in storage</exception>
        /// <exception cref="AccessDeniedException">If user have not permisions to read file</exception>
        /// <exception cref="IntegrationException">Other storage exeptions</exception>
        public async Task<Stream> ReadFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken = default)
        {
            var fileinfo = await FindFileAsync(bucketName, fileId, cancellationToken);
            if (fileinfo != null)
            {
                try
                {
                    var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString());
                    using var file = File.OpenRead(filePath);

                    var ms = new MemoryStream();
                    file.CopyTo(ms);

                    return ms;
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new AccessDeniedException(ex);
                }
                catch (Exception ex)
                {
                    throw new IntegrationException(ex);
                }
            }
            else throw new NotFoundException(null);
        }

        /// <summary>
        /// Deletes file from storage
        /// </summary>
        /// <param name="fileId">Id of file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>true - if file deletes, false - if not</returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="IntegrationException"></exception>
        public async Task<bool> DeleteFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken = default)
        {
            var fileInfo = await FindFileAsync(bucketName, fileId, cancellationToken);

            var metadataPath = Path.Combine(folderConfiguration.MetadataPath, fileId.ToString() + ".json");

            var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString());

            if (File.Exists(metadataPath) && File.Exists(filePath))
            {
                try
                {
                    File.Delete(metadataPath);
                    File.Delete(filePath);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else throw new NotFoundException(new ArgumentException($"File or metadata with key {fileId} does not exist"));
        }

        #endregion

        #region IDisposable members

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        { }

        #endregion
    }
}
