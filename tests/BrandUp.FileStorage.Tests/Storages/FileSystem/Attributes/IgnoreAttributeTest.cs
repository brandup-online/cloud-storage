using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Attributes;

namespace BrandUp.FileStorage.Tests.Storages.FileSystem.Attributes
{
    public class IgnoreAttributeTest : FileSystemClientTestBase<IgnoreFakeFile>
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
            var fakeFileToAttributedFakeFile = await TestGetAsync(metadata.FileId);
            var attributedFakeFileToFakeFile = await noIgnoreAttributeClient.TestGetAsync(attributedMetadata.FileId);

            Assert.Equal(metadata.Metadata.FileName, attributedMetadata.Metadata.FileName);
            Assert.Equal(metadata.Metadata.Data, attributedMetadata.Metadata.Data);

            await TestDeleteAsync(metadata.FileId);
            await noIgnoreAttributeClient.TestDeleteAsync(attributedMetadata.FileId);
        }
    }

    public class IgnoreFakeFile : IFileMetadata
    {
        public int Data { get; set; }
        public string FileName { get; set; }

        [MetadataIgnore]
        public bool HaveData { get; set; }
    }

    public class NoIgnoreAttributeTestClient : FileSystemClientTestBase<NoIgnoreFakeFile>
    {
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
