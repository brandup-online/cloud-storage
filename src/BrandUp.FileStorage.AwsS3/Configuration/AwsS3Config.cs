using BrandUp.FileStorage.Abstract.Configuration;

namespace BrandUp.FileStorage.AwsS3.Configuration
{
    /// <summary>
    /// Class for store configuration about Amazon S3 cloud storage
    /// </summary>
    public class AwsS3Configuration : IFileStorageConfiguration, IFileMetadataConfiguration
    {
        /// <summary>
        /// Url of service
        /// </summary>
        public string ServiceUrl { get; set; }
        /// <summary>
        /// Authentication region
        /// </summary>
        public string AuthenticationRegion { get; set; }
        /// <summary>
        /// Access key Id
        /// </summary>
        public string AccessKeyId { get; set; }
        /// <summary>
        /// Secret access key
        /// </summary>
        public string SecretAccessKey { get; set; }
        /// <summary>
        /// Name of the bucket 
        /// </summary>
        public string BucketName { get; set; }

        internal IDictionary<string, AwsS3Configuration> InnerConfiguration { get; set; }

        IDictionary<string, IFileMetadataConfiguration> IFileStorageConfiguration.InnerConfiguration
            => InnerConfiguration.ToDictionary(k => k.Key, v => (IFileMetadataConfiguration)v.Value);
    }
}