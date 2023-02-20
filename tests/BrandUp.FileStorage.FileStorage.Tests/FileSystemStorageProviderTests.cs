using BrandUp.FileStorage.Builder;
using BrandUp.FileStorage.FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.FileStorage.Tests
{
    public class FileSystemStorageProviderTests : FileStorageTestBase
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
            builder.AddFolderStorage("folder", options =>
                 {
                     options.MetadataPath = Path.Combine(TestDirectiory, "metadata");
                     options.ContentPath = Path.Combine(TestDirectiory, "content");
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