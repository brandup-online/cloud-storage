using CloudStorage.Files.Interfaces;

namespace CloudStorage.AwsS3.Configuration
{
    public class AwsS3Config : IStorageConfig
    {
        public string ServiceUrl { get; set; }
        public string AuthenticationRegion { get; set; }
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string BucketName { get; set; }
    }
}
