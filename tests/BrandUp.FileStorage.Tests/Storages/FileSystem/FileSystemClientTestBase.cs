using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests.Storages.FileSystem
{
    public abstract class FileSystemClientTestBase<T> : FileStorageTest<T> where T : class, IFileMetadata, new()
    {
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddFolderStorage(Configuration.GetSection("TestFolderStorage"))
            .AddFolderFor<T>();


            base.OnConfigure(services, builder);
        }

    }
}

