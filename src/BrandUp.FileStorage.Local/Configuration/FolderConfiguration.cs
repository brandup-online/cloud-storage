using BrandUp.FileStorage.Abstract.Configuration;

namespace BrandUp.FileStorage.Folder.Configuration
{
    /// <summary>
    /// Configuration for Folder storage
    /// </summary>
    public class FolderConfiguration : IFileStorageConfiguration, IFileMetadataConfiguration
    {

        /// <summary>
        /// Path to folder with files
        /// </summary>
        public string ContentPath { get; set; }
        /// <summary>
        /// Path to folder with matadata
        /// </summary>
        public string MetadataPath { get; set; }

        internal IDictionary<string, FolderConfiguration> InnerConfiguration { get; set; }

        IDictionary<string, IFileMetadataConfiguration> IFileStorageConfiguration.InnerConfiguration
            => InnerConfiguration.ToDictionary(k => k.Key, v => (IFileMetadataConfiguration)v.Value);
    }
}
