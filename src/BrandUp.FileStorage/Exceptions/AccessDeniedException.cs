namespace BrandUp.FileStorage.Exceptions
{
    /// <summary>
    /// Exception that throws if storage denies access to itself (invalid configuration or something else)
    /// </summary>
    public class AccessDeniedException : Exception
    {
        /// <summary>
        /// Exception that throws if storage denies access to itself (invalid configuration or something else)
        /// </summary>
        /// <param name="innerException">Inner exception</param>
        public AccessDeniedException(Exception innerException) : base("Access denied", innerException) { }
    }
}
