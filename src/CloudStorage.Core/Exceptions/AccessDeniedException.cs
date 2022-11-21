using Amazon.S3;

namespace BrandUp.CloudStorage.Exceptions
{
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(AmazonS3Exception innerException) : base("Доступ запрещен", innerException) { }
    }
}
