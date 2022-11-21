using Amazon.S3;

namespace BrandUp.CloudStorage.Exceptions
{
    public class IntegrationException : Exception
    {
        public IntegrationException(AmazonS3Exception innerException) : base("Ошибка взаимодействия.", innerException) { }
    }
}
