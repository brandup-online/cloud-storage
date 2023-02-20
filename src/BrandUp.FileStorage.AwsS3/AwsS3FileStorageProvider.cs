using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.Exceptions;

namespace BrandUp.FileStorage.AwsS3
{
    /// <summary>
    /// Storage for Amazon S3 cloud storage
    /// </summary>
    public class AwsS3FileStorageProvider : IStorageProvider
    {
        readonly AwsS3Configuration options;
        readonly AmazonS3Client client;
        bool isDisposed;

        const string AwsMetadataPrefix = "X-Amz-Meta";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options">Amazon S3 configuration</param>
        /// <exception cref="ArgumentNullException"></exception>
        public AwsS3FileStorageProvider(AwsS3Configuration options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            client = new AmazonS3Client(this.options.AccessKeyId, this.options.SecretAccessKey,
                new AmazonS3Config
                {
                    ServiceURL = this.options.ServiceUrl,
                    AuthenticationRegion = this.options.AuthenticationRegion,
                });
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
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms, cancellationToken);
            ms.Seek(0, SeekOrigin.Begin);

            if (ms.Length == 0)
                throw new InvalidOperationException("File does not contain any data");
            var innerDictionary = metadata.ToDictionary(k => AwsMetadataPrefix + k.Key.ToTrainCase(), v => v);

            try
            {
                using var fileTransferUtility = new TransferUtility(client);
                var transferUtilityUploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    Key = fileId.ToString().ToLower(),
                    InputStream = ms
                };

                return await FindFileAsync(bucketName, fileId, cancellationToken);
            }
            catch (AmazonS3Exception ex)
            {
                throw ex.StatusCode switch
                {
                    System.Net.HttpStatusCode.Forbidden => new AccessDeniedException(ex),
                    System.Net.HttpStatusCode.Conflict => new ArgumentException($"File with key {fileId} already exist"),
                    _ => new IntegrationException(ex),
                };
            }
        }

        /// <summary>
        /// Gets metadata of file
        /// </summary>
        /// <param name="bucketName">Id of file</param>
        /// <param name="fileId">Id of file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Information of file with metadata</returns>
        /// <exception cref="AccessDeniedException"></exception>
        /// <exception cref="IntegrationException"></exception>
        public async Task<FileInfo> FindFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = bucketName,
                    Key = fileId.ToString().ToLower()

                }, cancellationToken);

                var metadata = response.Metadata.Keys.ToDictionary(k => k.Replace(AwsMetadataPrefix, "").ToPascalCase(), v => response.Metadata[v]);

                return new FileInfo(fileId, response.ContentLength, metadata);
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

        /// <summary>
        /// Reads file from storage
        /// </summary>
        /// <param name="bucketName">Id of file</param>
        /// <param name="fileId">Id of file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>File stream</returns>
        /// <exception cref="NotFoundException">If file does not exist in storage</exception>
        /// <exception cref="AccessDeniedException">If user have not permisions to read file</exception>
        /// <exception cref="IntegrationException">Other storage exeptions</exception>
        public async Task<Stream> ReadFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await client.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileId.ToString().ToLower()

                }, cancellationToken);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex)
            {
                return ex.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => throw new NotFoundException(fileId),
                    System.Net.HttpStatusCode.Forbidden => throw new AccessDeniedException(ex),
                    _ => throw new IntegrationException(ex)
                };
            }
        }

        /// <summary>
        /// Deletes file from storage
        /// </summary>
        /// <param name="bucketName">Id of file</param>
        /// <param name="fileId">Id of file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>true - if file deletes, false - if not</returns>
        /// <exception cref="IntegrationException"></exception>
        public async Task<bool> DeleteFileAsync(string bucketName, Guid fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                await client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = bucketName,
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

        /// <summary>
        /// Dispose Amazon client
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                    client.Dispose();

                isDisposed = true;
            }
        }

        /// <summary>
        /// Dispose Amazon client
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
