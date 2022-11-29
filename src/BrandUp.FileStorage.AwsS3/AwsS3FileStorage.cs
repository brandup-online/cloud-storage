using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.Exceptions;

namespace BrandUp.FileStorage.AwsS3
{
    public class AwsS3FileStorage<TMetadata> : IFileStorage<TMetadata> where TMetadata : class, IFileMetadata, new()
    {
        readonly AwsS3Configuration options;
        readonly AmazonS3Client client;
        readonly IMetadataSerializer<TMetadata> metadataSerializer;

        private bool isDisposed;

        public AwsS3FileStorage(AwsS3Configuration options, IMetadataSerializer<TMetadata> metadataSerializer)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.metadataSerializer = metadataSerializer ?? throw new ArgumentNullException(nameof(metadataSerializer));

            client = new AmazonS3Client(this.options.AccessKeyId, this.options.SecretAccessKey,
                new AmazonS3Config
                {
                    ServiceURL = this.options.ServiceUrl,
                    AuthenticationRegion = this.options.AuthenticationRegion,
                });
        }

        #region IFileStorage members

        public async Task<FileInfo<TMetadata>> UploadFileAsync(Guid fileId, TMetadata fileInfo, Stream fileStream, CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms, cancellationToken);
            ms.Seek(0, SeekOrigin.Begin);

            if (ms.Length == 0)
                throw new InvalidOperationException("File does not contain any data");

            try
            {
                using var fileTransferUtility = new TransferUtility(client);
                var transferUtilityUploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = options.BucketName,
                    Key = fileId.ToString().ToLower(),
                    InputStream = ms
                };

                var metadataDictionary = metadataSerializer.Serialize(fileInfo);

                foreach (var pair in metadataDictionary)
                    transferUtilityUploadRequest.Metadata.Add(pair.Key, pair.Value);

                transferUtilityUploadRequest.WithAutoCloseStream(true);

                await fileTransferUtility.UploadAsync(transferUtilityUploadRequest, cancellationToken);

                return await GetFileInfoAsync(fileId, cancellationToken);
            }
            catch (AmazonS3Exception ex)
            {
                throw ex.StatusCode switch
                {
                    System.Net.HttpStatusCode.Forbidden => new AccessDeniedException(ex),
                    _ => new IntegrationException(ex),
                };
            }
        }

        public Task<FileInfo<TMetadata>> UploadFileAsync(TMetadata fileInfo, Stream fileStream, CancellationToken cancellationToken = default)
             => UploadFileAsync(Guid.NewGuid(), fileInfo, fileStream, cancellationToken);


        //public Task<FileInfo<TMetadata>> UploadFileAsync(Guid fileId, Stream fileStream, CancellationToken cancellationToken = default)
        //{
        //    return UploadFileAsync(fileId, new TMetadata(), fileStream, cancellationToken);
        //}

        //public Task<FileInfo<TMetadata>> UploadFileAsync(Stream fileStream, CancellationToken cancellationToken = default)
        //{
        //    return UploadFileAsync(new TMetadata(), fileStream, cancellationToken);
        //}

        public async Task<FileInfo<TMetadata>> GetFileInfoAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = options.BucketName,
                    Key = fileId.ToString().ToLower()

                }, cancellationToken);

                return metadataSerializer.Deserialize(fileId, response);
            }
            catch (AmazonS3Exception ex)
            {
                return ex.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => null,
                    System.Net.HttpStatusCode.Forbidden => throw new AccessDeniedException(ex),
                    _ => throw new IntegrationException(ex)
                };
            }
        }

        public async Task<Stream> ReadFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = options.BucketName,
                    Key = fileId.ToString().ToLower()

                }, cancellationToken);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex)
            {
                return ex.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => null,
                    System.Net.HttpStatusCode.Forbidden => throw new AccessDeniedException(ex),
                    _ => throw new IntegrationException(ex)
                };
            }
        }

        public async Task<bool> DeleteFileAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                await client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = options.BucketName,
                    Key = fileId.ToString().ToLower()
                }, cancellationToken);

                return true;
            }
            catch (AmazonS3Exception ex)
            {
                return ex.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => false,
                    System.Net.HttpStatusCode.Forbidden => false,
                    _ => throw new IntegrationException(ex)
                };
            }
        }

        #endregion

        #region IDisposable members

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                    client.Dispose();

                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
