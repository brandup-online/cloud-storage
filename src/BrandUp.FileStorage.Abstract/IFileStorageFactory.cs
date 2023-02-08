namespace BrandUp.FileStorage.Abstract
{
    /// <summary>
    /// Factory class for IFileStorage
    /// </summary>
    public interface IFileStorageFactory
    {
        /// <summary>
        /// Method creates instanse of storage by config type
        /// </summary>
        /// <typeparam name="TFileType">File for that creates storage</typeparam>
        /// <typeparam name="TConfigType">Configuration is defines type of storage</typeparam>
        /// <returns>instance of IFileStorage</returns>
        public IFileStorage<TFileType> Create<TFileType>() where TFileType : class, IFileMetadata, new();
    }
}
