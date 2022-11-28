using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.AwsS3.Context;
using BrandUp.FileStorage.Exceptions;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BrandUp.FileStorage.AwsS3
{
    public class AwsS3CloudClient<TMetadata> : ICloudClient<TMetadata> where TMetadata : class, new()
    {
        readonly AwsS3Config options;
        readonly AmazonS3Client client;

        private bool isDisposed;

        const string metadataKey = "X-Amz-Meta";
        readonly PropertyInfo[] metadataProperties;

        public AwsS3CloudClient(IOptions<AwsS3Config> options, IAwsS3StorageContext storageContext)
        {
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            metadataProperties = storageContext.TypeMetaData[typeof(TMetadata)];

            if (storageContext.BucketConfigs.TryGetValue(typeof(TMetadata), out var bucketConfig))
            {
                if (bucketConfig.ServiceUrl != null)
                    this.options.ServiceUrl = bucketConfig.ServiceUrl;
                if (bucketConfig.AccessKeyId != null)
                    this.options.AccessKeyId = bucketConfig.AccessKeyId;
                if (bucketConfig.SecretAccessKey != null)
                    this.options.SecretAccessKey = bucketConfig.SecretAccessKey;
                if (bucketConfig.AuthenticationRegion != null)
                    this.options.AuthenticationRegion = bucketConfig.AuthenticationRegion;
                if (bucketConfig.BucketName != null)
                    this.options.BucketName = bucketConfig.BucketName;
            }

            client = new AmazonS3Client(this.options.AccessKeyId, this.options.SecretAccessKey,
                new AmazonS3Config
                {
                    ServiceURL = this.options.ServiceUrl,
                    AuthenticationRegion = this.options.AuthenticationRegion,
                });
        }

        #region ICloudStorage members

        public async Task<FileInfo<TMetadata>> UploadFileAsync(Guid fileId, TMetadata fileInfo, Stream fileStream, CancellationToken cancellationToken = default)
        {
            foreach (var property in metadataProperties)
                if (property.GetValue(fileInfo) == null)
                    throw new InvalidOperationException(property.Name);

            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms, cancellationToken);
            ms.Seek(0, SeekOrigin.Begin);

            if (ms.Length == 0)
                throw new InvalidOperationException("Файл не содержит данных.");

            try
            {
                using var fileTransferUtility = new TransferUtility(client);
                var transferUtilityUploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = options.BucketName,
                    Key = fileId.ToString().ToLower(),
                    InputStream = ms
                };

                var metadataDictionary = CreateMetadata(fileInfo);

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
        {
            return UploadFileAsync(Guid.NewGuid(), fileInfo, fileStream, cancellationToken);
        }

        public Task<FileInfo<TMetadata>> UploadFileAsync(Guid fileId, Stream fileStream, CancellationToken cancellationToken = default)
        {
            return UploadFileAsync(fileId, new TMetadata(), fileStream, cancellationToken);
        }

        public Task<FileInfo<TMetadata>> UploadFileAsync(Stream fileStream, CancellationToken cancellationToken = default)
        {
            return UploadFileAsync(new TMetadata(), fileStream, cancellationToken);
        }


        public async Task<FileInfo<TMetadata>> GetFileInfoAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = options.BucketName,
                    Key = fileId.ToString().ToLower()

                }, cancellationToken);

                return GenerateFileInfoFromRespnse(fileId, response);
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

        #region Helpers

        Dictionary<string, string> CreateMetadata(TMetadata fileInfo)
        {
            var metadata = new Dictionary<string, string>();

            foreach (var property in metadataProperties)
            {
                if (property.PropertyType == typeof(string))
                    metadata.Add(metadataKey + "-" + ToKebabCase(property.Name), EncodeFileName(property.GetValue(fileInfo).ToString()));
                else
                    metadata.Add(metadataKey + "-" + ToKebabCase(property.Name), property.GetValue(fileInfo).ToString());
            }

            return metadata;
        }

        FileInfo<TMetadata> GenerateFileInfoFromRespnse(Guid fileId, GetObjectMetadataResponse response)
        {
            var fileMetadata = new TMetadata();

            foreach (var property in metadataProperties)
            {
                var converter = TypeDescriptor.GetConverter(property.PropertyType);

                if (property.PropertyType == typeof(string))
                    property.SetValue(fileMetadata, DecodeFileName(response.Metadata[metadataKey + "-" + ToKebabCase(property.Name)]));
                else
                    property.SetValue(fileMetadata, converter.ConvertFrom(response.Metadata[metadataKey + "-" + ToKebabCase(property.Name)]));
            }

            return new FileInfo<TMetadata> { Metadata = fileMetadata, Size = response.ContentLength, FileId = fileId };

        }

        static string EncodeFileName(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            return Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(fileName));
        }
        static string DecodeFileName(string encodedValue)
        {
            if (encodedValue == null)
                throw new ArgumentNullException(nameof(encodedValue));

            return System.Text.Encoding.UTF8.GetString(Convert.FromHexString(encodedValue));
        }

        static string ToKebabCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(
                value,
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])",
                "-$1",
                RegexOptions.Compiled)
                .Trim();
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
