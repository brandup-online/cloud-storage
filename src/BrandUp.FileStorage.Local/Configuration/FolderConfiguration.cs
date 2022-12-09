namespace BrandUp.FileStorage.Folder.Configuration
{
    /// <summary>
    /// Configuration for Folder storage
    /// </summary>
    public class FolderConfiguration
    {
        /// <summary>
        /// Path to folder with files
        /// </summary>
        public string ContentPath { get; set; }
        /// <summary>
        /// Path to folder with matadata
        /// </summary>
        public string MetadataPath { get; set; }
    }
}
