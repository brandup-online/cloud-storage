using BrandUp.FileStorage.Folder.Configuration;

namespace BrandUp.FileStorage.Folder
{
    /// <summary>
    /// 
    /// </summary>
    public static class IFileStorageFactoryExtention
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFile"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IFileStorage<TFile> CreateFolderStorage<TFile>(this IFileStorageFactory factory) where TFile : class, IFileMetadata, new()
            => factory.Create<TFile, FolderConfiguration>();

    }
}
