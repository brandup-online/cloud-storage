using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.FileSystem
{
    public class FileSystemStorageProviderTests_Image : FileStorageTests
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
                .AddFolderStorage("image", options =>
                {
                    options.MetadataPath = Path.Combine(TestDirectiory, "metadata");
                    options.ContentPath = Path.Combine(TestDirectiory, "content");
                    options.DefaultExtension = "jpg";
                });

            services
                .AddFileContext<TestFileContext>("image");
        }

        protected override Task OnFinishAsync(IServiceProvider rootServices, IServiceProvider scopeServices)
        {
            Directory.Delete(TestDirectiory, true);
            return base.OnFinishAsync(rootServices, scopeServices);
        }

        #endregion
    }
}
