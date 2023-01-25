namespace BrandUp.FileStorage.Exceptions
{
    /// <summary>
    /// Exception that throws if file that you trying to get does not exist in this storage
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Exception constructor with inner exception.
        /// </summary>
        /// <param name="innerException">Inner exception.</param>
        public NotFoundException(Exception innerException) : base("File not found", innerException) { }
        /// <summary>
        ///  Exception constructor with file id.
        /// </summary>
        /// <param name="fileId">file id.</param>
        public NotFoundException(Guid fileId) : base($"File with id {fileId} not found") { }
    }
}
