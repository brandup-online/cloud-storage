using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Attributes;
using BrandUp.FileStorage.AwsS3;
using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests.Storages.Aws.Attributes
{
    public class IgnoreAttributeTest : AwsClientTestBase<IgnoreFakeFile>
    {
        internal override IgnoreFakeFile CreateMetadataValue()
        {
            return new IgnoreFakeFile()
            {
                Data = 1,
                FileName = "test"
            };
        }

        [Fact]
        public async Task Success()
        {
            var noIgnoreAttributeClient = new NoIgnoreAttributeTestClient();
            using var ms = new MemoryStream(image);
            var metadata = await noIgnoreAttributeClient.TestUploadAsync(noIgnoreAttributeClient.CreateMetadataValue(), ms);
            using var ms1 = new MemoryStream(image);
            var attributedMetadata = await TestUploadAsync(CreateMetadataValue(), ms1);

            //Gets IgnoreFakeFile metadata as NoIgnoreFakeFile metadata and NoIgnoreFakeFile metadata as IgnoreAttributeTest metadata
            //Because for AwsStorage they represents equivalent metadata keys it's working.
            var fakeFileToAttributedFakeFile = await TestGetAsync(metadata.Id);
            var attributedFakeFileToFakeFile = await noIgnoreAttributeClient.TestGetAsync(attributedMetadata.Id);

            Assert.Equal(metadata.Metadata.FileName, attributedMetadata.Metadata.FileName);
            Assert.Equal(metadata.Metadata.Data, attributedMetadata.Metadata.Data);

            await TestDeleteAsync(metadata.Id);
            await noIgnoreAttributeClient.TestDeleteAsync(attributedMetadata.Id);
        }
    }

    public class IgnoreFakeFile : IFileMetadata
    {
        public int Data { get; set; }
        public string FileName { get; set; }

        [MetadataIgnore]
        public bool HaveData { get; set; }
    }

    public class NoIgnoreAttributeTestClient : FileStorageTestBase<NoIgnoreFakeFile>
    {
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddAwsS3Storage(Configuration.GetSection("TestCloudStorage"))
                     .AddAwsS3Bucket<NoIgnoreFakeFile>("FakeAwsFile");

            base.OnConfigure(services, builder);
        }

        internal override NoIgnoreFakeFile CreateMetadataValue()
        {
            return new NoIgnoreFakeFile()
            {
                Data = 1,
                FileName = "test"
            };
        }
    }

    public class NoIgnoreFakeFile : IFileMetadata
    {
        public int Data { get; set; }
        public string FileName { get; set; }

    }
}
