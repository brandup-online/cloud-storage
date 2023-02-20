using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.AwsS3.Tests
{
    public class AwsS3FileStorageProviderTests : FileStorageTestBase
    {
        protected override void OnConfigurationBuilding(IConfigurationBuilder builder)
        {
            builder.AddUserSecrets(typeof(AwsS3FileStorageProviderTests).Assembly);
        }

        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddAwsS3Storage("aws", options =>
                 {
                     Configuration.GetSection("TestCloudStorage:Default").Bind(options);
                 });

            services
                .AddFileContext<TestFileContext>("aws");
        }
    }
}