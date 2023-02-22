using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.Builder;
using BrandUp.FileStorage.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.AwsS3
{
    public class AwsS3FileStorageProviderTests : FileStorageTests
    {
        public AwsS3FileStorageProviderTests()
        {
            var options = new AwsS3Configuration();
            Configuration.GetSection("TestCloudStorage:Default").Bind(options);
            fileStorage = new AwsS3FileStorageProvider(options);
        }

        /// <summary>
        /// All dictionary transformation is correct.
        /// </summary>
        [Fact]
        public async void CorrectMetadata()
        {
            Dictionary<string, string> metadata = new()
            {
                {"FileName" , "Test" },
                {"Size" , "100" },
                {"Id" , Guid.NewGuid().ToString() },
                {"CreatedDate" , DateTime.UtcNow.Date.ToString() }
            };

            using MemoryStream ms = new(image);
            var bucket = (string)Configuration.GetValue(typeof(string), "TestCloudStorage:FakeAwsFile:TempFiles");
            var result = await fileStorage.UploadFileAsync(bucket, Guid.NewGuid(), metadata, ms, CancellationToken.None);
            Assert.NotNull(result);
            foreach (var pair in result.Metadata)
            {
                Assert.Contains(pair.Key, metadata.Keys);
                Assert.Equal(pair.Value, metadata[pair.Key]);
            }
        }

        #region FileStorageTests members

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

        #endregion
    }
}