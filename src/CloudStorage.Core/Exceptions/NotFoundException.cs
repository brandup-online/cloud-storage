using Amazon.S3;

namespace BrandUp.FileStorage.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(AmazonS3Exception innerException) : base("Файл не найден", innerException) { }
    }
}
