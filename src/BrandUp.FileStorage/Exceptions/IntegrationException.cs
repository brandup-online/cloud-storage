namespace BrandUp.FileStorage.Exceptions
{
    public class IntegrationException : Exception
    {
        public IntegrationException(Exception innerException) : base("Integration error", innerException) { }
    }
}
