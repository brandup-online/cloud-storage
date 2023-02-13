using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Attributes;

namespace BrandUp.FileStorage.Tests.Storages.Aws.Attributes
{
    public class KeyAttributeClientTest : AwsClientTestBase<AttributedFakeFile>
    {
        #region FileStorageTestBase members

        internal override AttributedFakeFile CreateMetadataValue()
        {
            return new AttributedFakeFile
            {
                Inner = new() { InnerGuid = TestGuid, Bool = true },
                FileName = "string",
                Ext = "png",
                DateTime = new DateTime(2002, 12, 15, 13, 45, 0),
                Int = 21332,
                FakeSpan = TimeSpan.FromSeconds(127)
            };
        }

        #endregion

        #region Tests

        /// <summary>
        /// <see cref="FakeFile"/> and <see cref="AttributedFakeFile"/> should have the same metadata keys.
        /// </summary>
        [Fact]
        public async Task Success_CheckKeys()
        {
            var fakeFileClient = new AwsClientTest();
            using var ms = new MemoryStream(image);
            var metadata = await fakeFileClient.TestUploadAsync(fakeFileClient.CreateMetadataValue(), ms);
            using var ms1 = new MemoryStream(image);
            var attributedMetadata = await TestUploadAsync(CreateMetadataValue(), ms1);

            //Gets FakeFile metadata as AttributedFakeFile metadata and AttributedFakeFile metadata FakeFile metadata
            //Because for AwsStorage they represents equivalent metadata keys it's working.
            var fakeFileToAttributedFakeFile = await TestGetAsync(metadata.FileId);
            var attributedFakeFileToFakeFile = await fakeFileClient.TestGetAsync(attributedMetadata.FileId);

            Assert.Equal(metadata.Metadata.FileName, attributedMetadata.Metadata.FileName);
            Assert.Equal(metadata.Metadata.Extension, attributedMetadata.Metadata.Ext);
            Assert.Equal(metadata.Metadata.FakeInt, attributedMetadata.Metadata.Int);
            Assert.Equal(metadata.Metadata.FakeDateTime, attributedMetadata.Metadata.DateTime);
            Assert.Equal(metadata.Metadata.FakeTimeSpan, attributedMetadata.Metadata.FakeSpan);
            Assert.Equal(metadata.Metadata.FakeInner.FakeBool, attributedMetadata.Metadata.Inner.Bool);
            Assert.Equal(metadata.Metadata.FakeInner.FakeGuid, attributedMetadata.Metadata.Inner.InnerGuid);

            await TestDeleteAsync(metadata.FileId);
            await fakeFileClient.TestDeleteAsync(attributedMetadata.FileId);
        }

        #endregion
    }

    public class AttributedFakeFile : IFileMetadata
    {
        public string FileName { get; set; }
        [MetadataKey("Extension")]
        public string Ext { get; set; }
        [MetadataKey("fake-int")]
        public int Int { get; set; }
        [MetadataKey("fake date time")]
        public DateTime DateTime { get; set; }
        [MetadataKey("fake_time_Span")]
        public TimeSpan FakeSpan { get; set; }
        [MetadataKey("Fake_INNer")]
        public AttributtedInnerClass Inner { get; set; }
    }

    public class AttributtedInnerClass
    {
        [MetadataKey("FakeBool")]
        public bool Bool { get; set; }
        [MetadataKey("Fake      - guid")]
        public Guid InnerGuid { get; set; }
    }
}
