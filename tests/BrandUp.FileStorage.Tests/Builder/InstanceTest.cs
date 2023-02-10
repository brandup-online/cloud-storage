using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.AwsS3;
using BrandUp.FileStorage.Folder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Builder
{
    public class InstanceTest : FileStorageTestBase
    {
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddAwsS3Storage(Configuration.GetSection("TestCloudStorage"))
                        .AddAwsS3Bucket<Storages.Aws._fakes.FakeFile>("FakeAwsFile");

            builder.AddFolderStorage(Configuration.GetSection("TestFolderStorage"))
                     .AddFolderFor<Storages.Local._fakes.FakeFile>("FakeAwsFile");

            base.OnConfigure(services, builder);
        }
        /// <summary>
        /// Scope returns only one instance of storage 
        /// </summary>
        [Fact]
        public void Succses_SameInstanses()
        {
            using var awsStorage1 = Services.GetRequiredService<IFileStorage<Storages.Aws._fakes.FakeFile>>();
            using var localStorage1 = Services.GetRequiredService<IFileStorage<Storages.Local._fakes.FakeFile>>();
            using var awsStorage2 = Services.GetRequiredService<IFileStorage<Storages.Aws._fakes.FakeFile>>();
            using var localStorage2 = Services.GetRequiredService<IFileStorage<Storages.Local._fakes.FakeFile>>();

            Assert.Same(awsStorage1, awsStorage2);
            Assert.Same(localStorage1, localStorage2);
        }
    }
}
