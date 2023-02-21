using BrandUp.FileStorage.Builder;
using BrandUp.FileStorage.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.AwsS3
{
    public class AwsS3FileStorageProviderTests : FileStorageTests
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
                .AddFileContext<TestFileContext>("aws", options =>
                {
                    options.FromConfiguration(Configuration.GetSection("TestCloudStorage:FakeAwsFile"));
                });
        }
    }
}