namespace BrandUp.FileStorage.Abstract.Exceptions
{
    /// <summary>
    /// Common exception for invalid storage work
    /// </summary>
    public class IntegrationException : Exception
    {
        /// <summary>
        /// Common exception for invalid storage work
        /// </summary>
        /// <param name="innerException">Inner exception</param>
        public IntegrationException(Exception innerException) : base("Integration error", innerException) { }
    }
}
