namespace BrandUp.FileStorage
{
    /// <summary>
    /// Interface of metadata
    /// </summary>
    public interface IFileMetadata
    {
        /// <summary>
        /// Name of file
        /// </summary>
        string FileName { get; }
        /// <summary>
        /// Extension of file
        /// </summary>
        string Extension { get; }
    }
}
