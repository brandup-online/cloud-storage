using BrandUp.FileStorage.Exceptions;
using BrandUp.FileStorage.Folder.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace BrandUp.FileStorage.Folder
{
    /// <summary>
    /// Storage for local file system
    /// </summary>
    /// <typeparam name="TFile"></typeparam>
    public class LocalFileStorage<TFile> : IFileStorage<TFile> where TFile : class, IFileMetadata, new()
    {
        readonly FolderConfiguration folderConfiguration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderConfiguration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LocalFileStorage(FolderConfiguration folderConfiguration)
        {
            this.folderConfiguration = folderConfiguration ?? throw new ArgumentNullException(nameof(folderConfiguration));

            if (!Directory.Exists(this.folderConfiguration.ContentPath))
                Directory.CreateDirectory(this.folderConfiguration.ContentPath);

            if (!Directory.Exists(this.folderConfiguration.MetadataPath))
                Directory.CreateDirectory(this.folderConfiguration.MetadataPath);
        }

        #region IFileStorage members

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
        public async Task<FileInfo<TFile>> UploadFileAsync(Guid fileId, TFile fileInfo, Stream fileStream, CancellationToken cancellationToken = default)
        {
            var ext = Path.GetExtension(fileInfo.FileName);
            var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString() + "." + ext);
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
                    var content = JsonConvert.SerializeObject(fileInfo, Formatting.Indented);
                    byte[] bytes = Encoding.ASCII.GetBytes(content);
                    await file.WriteAsync(bytes, cancellationToken);
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


            return new FileInfo<TFile>()
            {
                FileId = fileId,
                Size = fileStream.Length,
                Metadata = fileInfo
            };
        }

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
        public Task<FileInfo<TFile>> UploadFileAsync(TFile fileInfo, Stream fileStream, CancellationToken cancellationToken = default)
            => UploadFileAsync(Guid.NewGuid(), fileInfo, fileStream, cancellationToken);

        /// <summary>
        /// Gets metadata of file
        /// </summary>
        /// <param name="fileId">Id of file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Information of file with metadata</returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="AccessDeniedException"></exception>
        /// <exception cref="IntegrationException"></exception>
        public async Task<FileInfo<TFile>> GetFileInfoAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            var metadataPath = Path.Combine(folderConfiguration.MetadataPath, fileId.ToString() + ".json");

            if (File.Exists(metadataPath))
            {
                try
                {
                    using var metadata = File.OpenRead(metadataPath);
                    using var reader = new StreamReader(metadata);
                    var json = await reader.ReadToEndAsync();
                    var data = JsonConvert.DeserializeObject<TFile>(json);
                    var ext = Path.GetExtension(data.FileName);

                    var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString() + "." + ext);

                    return new FileInfo<TFile>()
                    {
                        FileId = fileId,
                        Size = new FileInfo(filePath).Length,
                        Metadata = data
                    };
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
        /// <param name="fileId">Id of file</param>
        /// <param name="cancellationToken">Cancellation tokSen</param>
        /// <returns>File stream</returns>
        /// <exception cref="NotFoundException">If file does not exist in storage</exception>
        /// <exception cref="AccessDeniedException">If user have not permisions to read file</exception>
        /// <exception cref="IntegrationException">Other storage exeptions</exception>
        public async Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            var fileinfo = await GetFileInfoAsync(fileId, cancellationToken);
            if (fileinfo != null)
            {
                try
                {
                    var ext = Path.GetExtension(fileinfo.Metadata.FileName);
                    var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString() + "." + ext);
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
        public async Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            var fileInfo = await GetFileInfoAsync(fileId, cancellationToken);

            var metadataPath = Path.Combine(folderConfiguration.MetadataPath, fileId.ToString() + ".json");

            var ext = Path.GetExtension(fileInfo.Metadata.FileName);
            var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString() + "." + ext);

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
