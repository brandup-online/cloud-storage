using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.FileSystem
{
    public class FileSystemStorageProviderTests : FileStorageTests
    {
        const string TestDirectiory = "C:\\test";

        #region FileStorageTestBase members

        protected override Task OnInitializeAsync(IServiceProvider rootServices, IServiceProvider scopeServices)
        {
            Directory.CreateDirectory(TestDirectiory);
            return base.OnInitializeAsync(rootServices, scopeServices);
        }

        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder
                .AddFolderStorage("folder", options =>
                 {
                     options.MetadataPath = Path.Combine(TestDirectiory, "metadata");
                     options.ContentPath = Path.Combine(TestDirectiory, "content");
                 })
                .AddFolderStorage("dat", options =>
                {
                    options.MetadataPath = Path.Combine(TestDirectiory, "metadata");
                    options.ContentPath = Path.Combine(TestDirectiory, "content");
                    options.DefaultExtension = "dat";
                })
                .AddFolderStorage("image", options =>
                {
                    options.MetadataPath = Path.Combine(TestDirectiory, "metadata");
                    options.ContentPath = Path.Combine(TestDirectiory, "content");
                    options.DefaultExtension = "jpg";
                });

            services
                .AddFileContext<TestFileContext>("folder");
        }

        protected override Task OnFinishAsync(IServiceProvider rootServices, IServiceProvider scopeServices)
        {
            Directory.Delete(TestDirectiory, true);
            return base.OnFinishAsync(rootServices, scopeServices);
        }

        #endregion
    }
}