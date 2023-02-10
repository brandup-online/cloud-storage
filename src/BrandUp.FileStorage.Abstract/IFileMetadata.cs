namespace BrandUp.FileStorage.Abstract
{
    /// <summary>
    /// Interface of metadata
    /// </summary>
    public interface IFileMetadata
    {
        /// <summary>
        /// Id of the file that can be used to get the file from storage
        /// </summary>
        public Guid FileId { get; init; }
        /// <summary>
        /// Name of file
        /// </summary>
        string FileName { get; }
    }
}