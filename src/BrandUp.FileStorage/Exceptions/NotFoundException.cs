namespace BrandUp.FileStorage.Exceptions
{
    /// <summary>
    /// Exception that throws if file that you trying to get does not exist in this storage
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Exception that throws if file that you trying to get does not exist in this storage
        /// </summary>
        /// <param name="innerException">Inner exception</param>
        public NotFoundException(Exception innerException) : base("File not found", innerException) { }
    }
}
