using Amazon.S3;

namespace BrandUp.FileStorage.Exceptions
{
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(AmazonS3Exception innerException) : base("Доступ запрещен", innerException) { }
    }
}
