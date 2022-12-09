namespace BrandUp.FileStorage
{
    /// <summary>
    /// Class with informations about file
    /// </summary>
    /// <typeparam name="T">Metadata type</typeparam>
    public class FileInfo<T> where T : class, IFileMetadata, new()
    {
        /// <summary>
        /// Id of the file that can be used to get the file from storage
        /// </summary>
        public Guid FileId { get; init; }
        /// <summary>
        /// Size of filr
        /// </summary>
        public long Size { get; init; }
        /// <summary>
        /// Data that will be stored as file metadata in storage
        /// </summary>
        public T Metadata { get; init; }
    }
}