using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Attributes;
using BrandUp.FileStorage.AwsS3;
using BrandUp.FileStorage.Builder;
using BrandUp.FileStorage.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests.Storages.FileSystem.Attributes
{
    public class RequiredAttributeTest : FileStorageTestBase<RequiredFakeFile>
    {
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddAwsS3Storage(Configuration.GetSection("TestCloudStorage"))
                    .AddAwsS3Bucket<RequiredFakeFile>("FakeAwsFile");

            base.OnConfigure(services, builder);
        }

        internal override RequiredFakeFile CreateMetadataValue()
        {
            return new()
            {
                FileName = "name",
                Text = null,
            };
        }

        [Fact]
        public async Task Success()
        {
            using var ms = new MemoryStream(image);

            await Assert.ThrowsAsync<RequiredMetadataException>(async () =>
            {
                await Client.UploadFileAsync(CreateMetadataValue(), ms);
            });
        }
    }

    public class RequiredFakeFile : IFileMetadata
    {
        public string FileName { get; set; }
        [MetadataRequired]
        public string Text { get; set; }
    }
}
