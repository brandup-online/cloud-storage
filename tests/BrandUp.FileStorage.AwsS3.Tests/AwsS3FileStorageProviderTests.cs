using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.AwsS3
{
    public class AwsS3FileStorageProviderTests : FileStorageTestBase
    {
        readonly TestFileContext testFileContext;

        public AwsS3FileStorageProviderTests()
        {
            testFileContext = Services.GetRequiredService<TestFileContext>();
        }

        #region FileStorageTestBase members

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

        [Fact]
        public async Task Success_Attributes()
        {
            #region Preparation 

            var collection = testFileContext.FileStorageTestFiles;
            var attrbutedCollection = testFileContext.AttributedTestFiles;

            TestFile file = new()
            {
                FileName = "Test",
                Size = 100,
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.Date,
            };

            using MemoryStream stream = new(image);

            #endregion

            var fileInfo = await TestUploadAsync(collection, file, stream);
            var attributedInfo = await TestGetAsync(attrbutedCollection, fileInfo.Id);

            var metadata = attributedInfo.Metadata;
            Assert.Equal(file.FileName, metadata.FileName);
            Assert.Equal(file.Size, metadata.Size);
            Assert.Equal(file.Id, metadata.MailingId);
            Assert.Equal(file.CreatedDate, metadata.CreatedDate);
        }
    }
}