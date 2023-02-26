using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.FileSystem
{
    public class FileSystemStorageProviderTests : FileStorageTestBase
    {
        const string TestDirectiory = "C:\\test";

        readonly TestFileContext testFileContext;

        public FileSystemStorageProviderTests()
        {
            testFileContext = Services.GetRequiredService<TestFileContext>();
        }

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

        [Fact]
        public async Task Succsess_CRUD()
        {
            #region Preparation 

            var collection = testFileContext.FileStorageTestFiles;

            TestFile file = new()
            {
                FileName = "Test",
                Size = 100,
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.Date,
            };

            using MemoryStream stream = new(image);

            #endregion

            await CRUD(collection, file, stream);
        }
    }
}