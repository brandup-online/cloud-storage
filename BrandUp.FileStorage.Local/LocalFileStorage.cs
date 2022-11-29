using BrandUp.FileStorage.Folder.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace BrandUp.FileStorage.Folder
{
    public class LocalFileStorage<TFile> : IFileStorage<TFile> where TFile : class, IFileMetadata, new()
    {
        readonly FolderConfiguration folderConfiguration;

        public LocalFileStorage(FolderConfiguration folderConfiguration)
        {
            this.folderConfiguration = folderConfiguration ?? throw new ArgumentNullException(nameof(folderConfiguration));

            if (!Directory.Exists(this.folderConfiguration.ContentPath))
                Directory.CreateDirectory(this.folderConfiguration.ContentPath);

            if (!Directory.Exists(this.folderConfiguration.MetadataPath))
                Directory.CreateDirectory(this.folderConfiguration.MetadataPath);
        }

        #region IFileStorage members

        public async Task<FileInfo<TFile>> UploadFileAsync(Guid fileId, TFile fileInfo, Stream fileStream, CancellationToken cancellationToken = default)
        {
            var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString() + "." + fileInfo.Extension);
            var metadataPath = Path.Combine(folderConfiguration.MetadataPath, fileId.ToString() + ".json");

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

            return new FileInfo<TFile>()
            {
                FileId = fileId,
                Size = fileStream.Length,
                Metadata = fileInfo
            };
        }

        public Task<FileInfo<TFile>> UploadFileAsync(TFile fileInfo, Stream fileStream, CancellationToken cancellationToken = default)
            => UploadFileAsync(Guid.NewGuid(), fileInfo, fileStream, cancellationToken);

        public async Task<FileInfo<TFile>> GetFileInfoAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            var metadataPath = Path.Combine(folderConfiguration.MetadataPath, fileId.ToString() + ".json");

            if (File.Exists(metadataPath))
            {
                using var metadata = File.OpenRead(metadataPath);
                using var reader = new StreamReader(metadata);
                var json = await reader.ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<TFile>(json);

                var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString() + "." + data.Extension);

                return new FileInfo<TFile>()
                {
                    FileId = fileId,
                    Size = new FileInfo(filePath).Length,
                    Metadata = data
                };
            }
            else throw new ArgumentException($"Metadata file with key {fileId} does not exist");
        }

        public async Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            var fileinfo = await GetFileInfoAsync(fileId, cancellationToken);
            if (fileinfo != null)
            {
                var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString() + "." + fileinfo.Metadata.Extension);
                using var file = File.OpenRead(filePath);

                var ms = new MemoryStream();
                file.CopyTo(ms);

                return ms;
            }
            else throw new ArgumentNullException(nameof(fileinfo));
        }

        public async Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            var fileInfo = await GetFileInfoAsync(fileId, cancellationToken);

            var metadataPath = Path.Combine(folderConfiguration.MetadataPath, fileId.ToString() + ".json");
            var filePath = Path.Combine(folderConfiguration.ContentPath, fileId.ToString() + "." + fileInfo.Metadata.Extension);

            if (File.Exists(metadataPath) && File.Exists(filePath))
            {
                try
                {
                    await Task.Run(() =>
                    {
                        File.Delete(metadataPath);
                        File.Delete(filePath);
                    });

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else throw new ArgumentException($"File or metadata with key {fileId} does not exist");
        }

        #endregion

        #region IDisposable members

        public void Dispose()
        { }

        #endregion
    }
}
