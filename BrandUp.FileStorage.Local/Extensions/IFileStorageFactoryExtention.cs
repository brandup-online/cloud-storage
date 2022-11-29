using BrandUp.FileStorage.Folder.Configuration;

namespace BrandUp.FileStorage.Folder
{
    public static class IFileStorageFactoryExtention
    {
        public static IFileStorage<TFile> CreateFolderStorage<TFile>(this IFileStorageFactory factory) where TFile : class, IFileMetadata, new()
            => factory.Create<TFile, FolderConfiguration>();

    }
}
