namespace BrandUp.FileStorage.Abstract
{
    public interface IStorageInstanceCreator
    {
        IFileStorage<T> CreateStorageInstance<T>(IServiceProvider serviceProvider) where T : class, IFileMetadata, new();
    }
}
