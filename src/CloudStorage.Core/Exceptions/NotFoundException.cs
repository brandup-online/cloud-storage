using Amazon.S3;

namespace BrandUp.CloudStorage.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(AmazonS3Exception innerException) : base("Файл не найден", innerException) { }
    }
}
