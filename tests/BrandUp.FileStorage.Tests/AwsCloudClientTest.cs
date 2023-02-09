using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Attributes;
using BrandUp.FileStorage.AwsS3;
using BrandUp.FileStorage.Tests._fakes.Aws;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests
{
    public class AwsCloudClientTest : FileStorageTestBase
    {
        readonly IFileStorageFactory factory;

        public AwsCloudClientTest()
        {
            factory = Services.GetRequiredService<IFileStorageFactory>();
        }

        /// <summary>
        /// Basic functionality test
        /// </summary>
        [Fact]
        public async Task Success_Basics()
        {
            using var stream = new MemoryStream(Properties.Resources.Image);
            using var client = factory.CreateAwsStorage<FakeAwsFile>();

            Assert.NotNull(client);

            await DoCRUD(client, new FakeAwsFile
            {
                FakeInner = new() { FakeGuid = Guid.NewGuid(), FakeBool = true },
                FileName = "string",
                Extension = "png",
                FakeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),
                FakeInt = 21332,
                FakeTimeSpan = TimeSpan.FromSeconds(127)
            }, stream);
        }

        /// <summary>
        /// Constructs situation if metadata class has been changed.
        /// </summary>
        [Fact]
        public async Task Success_Changed_Metadata()
        {
            #region Old to New 

            using var stream = new MemoryStream(Properties.Resources.Image);
            using var clientOld = factory.CreateAwsStorage<FakeMetadataOld>();

            var fileId = Guid.NewGuid();

            var oldFileInfo = await clientOld.UploadFileAsync(fileId, new()
            {
                FileName = "string",
                SomeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),
                SomeGuid = Guid.NewGuid(),
            }, stream);

            using var clientNew = factory.CreateAwsStorage<FakeMetadataNew>();

            var newFileInfo = await clientNew.GetFileInfoAsync(fileId);

            Assert.NotNull(newFileInfo);
            Assert.Equal(oldFileInfo.Metadata.FileName, newFileInfo.Metadata.FileName);
            Assert.Equal(oldFileInfo.Metadata.SomeDateTime, newFileInfo.Metadata.SomeDateTime);
            Assert.Equal(oldFileInfo.Metadata.SomeGuid, newFileInfo.Metadata.SomeGuid);

            Assert.True(await clientOld.DeleteFileAsync(fileId));

            #endregion

            #region New to Old

            using var stream2 = new MemoryStream(Properties.Resources.Image);

            fileId = Guid.NewGuid();

            newFileInfo = await clientNew.UploadFileAsync(fileId, new()
            {
                FileName = "string",
                SomeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),
                SomeGuid = Guid.NewGuid(),
            }, stream2);

            oldFileInfo = await clientOld.GetFileInfoAsync(fileId);

            Assert.NotNull(newFileInfo);
            Assert.Equal(oldFileInfo.Metadata.FileName, newFileInfo.Metadata.FileName);
            Assert.Equal(oldFileInfo.Metadata.SomeDateTime, newFileInfo.Metadata.SomeDateTime);
            Assert.Equal(oldFileInfo.Metadata.SomeGuid, newFileInfo.Metadata.SomeGuid);

            Assert.True(await clientNew.DeleteFileAsync(fileId));

            #endregion
        }

        /// <summary>
        /// Testing <see cref="MetadataKeyAttribute"/> 
        /// </summary>
        [Fact]
        public async Task Success_Attributes()
        {
            #region Base to Attributed

            using var stream = new MemoryStream(Properties.Resources.Image);
            using var client = factory.CreateAwsStorage<FakeAwsFile>();

            var fileId = Guid.NewGuid();

            var fileInfo = await client.UploadFileAsync(fileId, new FakeAwsFile
            {
                FakeInner = new() { FakeGuid = Guid.NewGuid(), FakeBool = true },
                FileName = "string",
                Extension = "png",
                FakeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),
                FakeInt = 21332,
                FakeTimeSpan = TimeSpan.FromSeconds(127)
            }, stream);

            using var clientAttributed = factory.CreateAwsStorage<AttributedFakeFile>();

            var attributedFileInfo = await clientAttributed.GetFileInfoAsync(fileId);

            Assert.Equal(fileInfo.Metadata.FileName, attributedFileInfo.Metadata.FileName);
            Assert.Equal(fileInfo.Metadata.Extension, attributedFileInfo.Metadata.Ext);
            Assert.Equal(fileInfo.Metadata.FakeInt, attributedFileInfo.Metadata.Int);
            Assert.Equal(fileInfo.Metadata.FakeTimeSpan, attributedFileInfo.Metadata.FakeSpan);
            Assert.Equal(fileInfo.Metadata.FakeInner.FakeGuid, attributedFileInfo.Metadata.Inner.InnerGuid);
            Assert.Equal(fileInfo.Metadata.FakeInner.FakeBool, attributedFileInfo.Metadata.Inner.Bool);
            Assert.Equal(fileInfo.Metadata.FakeDateTime, attributedFileInfo.Metadata.DateTime);

            Assert.True(await client.DeleteFileAsync(fileId));

            #endregion

            #region Attributed to Base

            using var stream2 = new MemoryStream(Properties.Resources.Image);

            fileId = Guid.NewGuid();

            attributedFileInfo = await clientAttributed.UploadFileAsync(fileId, new()

            {
                Inner = new() { InnerGuid = Guid.NewGuid(), Bool = true },
                FileName = "string",
                Ext = "png",
                DateTime = new DateTime(2002, 12, 15, 13, 45, 0),
                Int = 21332,
                FakeSpan = TimeSpan.FromSeconds(127)
            }, stream2);


            fileInfo = await client.GetFileInfoAsync(fileId);

            Assert.Equal(fileInfo.Metadata.FileName, attributedFileInfo.Metadata.FileName);
            Assert.Equal(fileInfo.Metadata.Extension, attributedFileInfo.Metadata.Ext);
            Assert.Equal(fileInfo.Metadata.FakeInt, attributedFileInfo.Metadata.Int);
            Assert.Equal(fileInfo.Metadata.FakeTimeSpan, attributedFileInfo.Metadata.FakeSpan);
            Assert.Equal(fileInfo.Metadata.FakeInner.FakeGuid, attributedFileInfo.Metadata.Inner.InnerGuid);
            Assert.Equal(fileInfo.Metadata.FakeInner.FakeBool, attributedFileInfo.Metadata.Inner.Bool);
            Assert.Equal(fileInfo.Metadata.FakeDateTime, attributedFileInfo.Metadata.DateTime);

            Assert.True(await client.DeleteFileAsync(fileId));

            #endregion
        }

        async Task DoCRUD<T>(IFileStorage<T> client, T metadata, Stream stream) where T : class, IFileMetadata, new()
        {
            var fileinfo = await client.UploadFileAsync(metadata, stream, CancellationToken.None);
            Assert.NotNull(fileinfo);

            var getFileinfo = await client.GetFileInfoAsync(fileinfo.FileId, CancellationToken.None);
            Assert.NotNull(getFileinfo);

            var inputMetadata = metadata as FakeAwsFile;
            var downloadedMetadata = getFileinfo.Metadata as FakeAwsFile;

            Assert.Equal(inputMetadata.FileName, downloadedMetadata.FileName);
            Assert.Equal(inputMetadata.Extension, downloadedMetadata.Extension);
            Assert.Equal(inputMetadata.FakeInt, downloadedMetadata.FakeInt);
            Assert.Equal(inputMetadata.FakeTimeSpan, downloadedMetadata.FakeTimeSpan);
            Assert.Equal(inputMetadata.FakeInner.FakeGuid, downloadedMetadata.FakeInner.FakeGuid);
            Assert.Equal(inputMetadata.FakeInner.FakeBool, downloadedMetadata.FakeInner.FakeBool);
            Assert.Equal(inputMetadata.FakeDateTime, downloadedMetadata.FakeDateTime);

            Assert.Equal(stream.Length, getFileinfo.Size);

            using var downlodadedStream = await client.ReadFileAsync(fileinfo.FileId, CancellationToken.None);
            Assert.NotNull(downlodadedStream);
            Assert.Equal(stream.Length, downlodadedStream.Length);

            var isDeleted = await client.DeleteFileAsync(fileinfo.FileId, CancellationToken.None);
            Assert.True(isDeleted);

            Assert.Null(await client.GetFileInfoAsync(fileinfo.FileId, CancellationToken.None));
        }
    }
}