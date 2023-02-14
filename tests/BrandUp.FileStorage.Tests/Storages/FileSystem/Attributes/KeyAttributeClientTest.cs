using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Attributes;
using BrandUp.FileStorage.Storages.FileSystem;

namespace BrandUp.FileStorage.Tests.Storages.FileSystem.Attributes
{
    public class KeyAttributeClientTest : FileSystemClientTestBase<AttributedFakeFile>
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
            var fakeFileClient = new FileSystemStorageTest();
            using var ms = new MemoryStream(image);
            var metadata = await fakeFileClient.TestUploadAsync(fakeFileClient.CreateMetadataValue(), ms);
            using var ms1 = new MemoryStream(image);
            var attributedMetadata = await TestUploadAsync(CreateMetadataValue(), ms1);

            //Gets FakeFile metadata as AttributedFakeFile metadata and AttributedFakeFile metadata FakeFile metadata
            //Because for AwsStorage they represents equivalent metadata keys it's working.
            var fakeFileToAttributedFakeFile = await TestGetAsync(metadata.Id);
            var attributedFakeFileToFakeFile = await fakeFileClient.TestGetAsync(attributedMetadata.Id);

            Assert.Equal(metadata.Metadata.FileName, attributedMetadata.Metadata.FileName);
            Assert.Equal(metadata.Metadata.Extension, attributedMetadata.Metadata.Ext);
            Assert.Equal(metadata.Metadata.FakeInt, attributedMetadata.Metadata.Int);
            Assert.Equal(metadata.Metadata.FakeDateTime, attributedMetadata.Metadata.DateTime);
            Assert.Equal(metadata.Metadata.FakeTimeSpan, attributedMetadata.Metadata.FakeSpan);
            Assert.Equal(metadata.Metadata.FakeInner.FakeBool, attributedMetadata.Metadata.Inner.Bool);
            Assert.Equal(metadata.Metadata.FakeInner.FakeGuid, attributedMetadata.Metadata.Inner.InnerGuid);

            await TestDeleteAsync(metadata.Id);
            await fakeFileClient.TestDeleteAsync(attributedMetadata.Id);
        }

        #endregion
    }

    public class AttributedFakeFile : IFileMetadata
    {
        public string FileName { get; set; }
        [MetadataProperty("Extension")]
        public string Ext { get; set; }
        [MetadataProperty("fake-int")]
        public int Int { get; set; }
        [MetadataProperty("fake date time")]
        public DateTime DateTime { get; set; }
        [MetadataProperty("fake_time_Span")]
        public TimeSpan FakeSpan { get; set; }
        [MetadataProperty("Fake_INNer")]
        public AttributtedInnerClass Inner { get; set; }
    }

    public class AttributtedInnerClass
    {
        [MetadataProperty("FakeBool")]
        public bool Bool { get; set; }
        [MetadataProperty("Fake      - guid")]
        public Guid InnerGuid { get; set; }
    }
}
