using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.AwsS3;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests.Storages.Aws
{
    public abstract class AwsClientTestBase<T> : FileStorageTest<T> where T : class, IFileMetadata, new()
    {
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddAwsS3Storage(Configuration.GetSection("TestCloudStorage"))
            .AddAwsS3Bucket<T>("FakeAwsFile");


            base.OnConfigure(services, builder);
        }

    }
}

