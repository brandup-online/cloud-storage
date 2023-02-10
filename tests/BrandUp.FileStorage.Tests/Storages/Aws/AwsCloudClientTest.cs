using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.AwsS3;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Storages.Aws
{
    public class AwsCloudClientTest : FileStorageTestBase
    {
        readonly byte[] image = Tests.Properties.Resources.Image;
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddAwsS3Storage(Configuration.GetSection("TestCloudStorage"))
                        .AddAwsS3Bucket<_fakes.FakeFile>("FakeAwsFile")
                        .AddAwsS3Bucket<_fakes.AttributedFakeFile>("FakeAwsFile")
                        .AddAwsS3Bucket<_fakes.FakeMetadataOld>("FakeAwsFile")
                        .AddAwsS3Bucket<_fakes.FakeMetadataNew>("FakeAwsFile")
                        .AddAwsS3Bucket<_fakes.FakeRequiredFile>("FakeAwsFile");
            base.OnConfigure(services, builder);
        }

        #region Basic tests

        /// <summary>
        /// Basic functionality test
        /// </summary>
        [Fact]
        public async Task Success_Basics()
        {
            using var stream = new MemoryStream(image);
            using var client = Services.GetRequiredService<IFileStorage<_fakes.FakeFile>>();

            Assert.NotNull(client);

            await DoCRUD(client, new _fakes.FakeFile
            {
                FakeInner = new() { FakeGuid = Guid.NewGuid(), FakeBool = true },
                FileName = "string",
                Extension = "png",
                FakeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),
                FakeInt = 21332,
                FakeTimeSpan = TimeSpan.FromSeconds(127)
            }, stream);
        }

        #endregion

        #region Attributes tests

        /// <summary>
        /// Constructs situation if metadata class has been changed.
        /// </summary>
        [Fact]
        public async Task Success_Changed_Metadata()
        {
            #region Old to New 

            using var stream = new MemoryStream(image);
            using var clientOld = Services.GetRequiredService<IFileStorage<_fakes.FakeMetadataOld>>();

            var fileId = Guid.NewGuid();

            var oldFileInfo = await clientOld.UploadFileAsync(fileId, new()
            {
                FileName = "string",
                SomeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),
                SomeGuid = Guid.NewGuid(),
            }, stream);

            using var clientNew = Services.GetRequiredService<IFileStorage<_fakes.FakeMetadataNew>>();

            var newFileInfo = await clientNew.GetFileInfoAsync(fileId);

            Assert.NotNull(newFileInfo);
            Assert.Equal(oldFileInfo.Metadata.FileName, newFileInfo.Metadata.FileName);
            Assert.Equal(oldFileInfo.Metadata.SomeDateTime, newFileInfo.Metadata.SomeDateTime);
            Assert.Equal(oldFileInfo.Metadata.SomeGuid, newFileInfo.Metadata.SomeGuid);

            Assert.True(await clientOld.DeleteFileAsync(fileId));

            #endregion

            #region New to Old

            using var stream2 = new MemoryStream(image);

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
        /// Testing attributes <see cref="MetadataKeyAttribute"/> and <see cref="MetadataIgnoreAttribute"/>
        /// </summary>
        [Fact]
        public async Task Success_Attributes()
        {
            #region Base to Attributed

            using var stream = new MemoryStream(image);
            using var client = Services.GetRequiredService<IFileStorage<_fakes.FakeFile>>();

            var fileId = Guid.NewGuid();

            var fileInfo = await client.UploadFileAsync(fileId, new _fakes.FakeFile
            {
                FakeInner = new() { FakeGuid = Guid.NewGuid(), FakeBool = true },
                FileName = "string",
                Extension = "png",
                FakeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),
                FakeInt = 21332,
                FakeTimeSpan = TimeSpan.FromSeconds(127)
            }, stream);

            using var clientAttributed = Services.GetRequiredService<IFileStorage<_fakes.AttributedFakeFile>>();

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

            using var stream2 = new MemoryStream(image);

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

        /// <summary>
        /// Test MetadataRequiredAttribute
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Required()
        {
            using var stream = new MemoryStream(image);
            using var client = Services.GetRequiredService<IFileStorage<_fakes.FakeRequiredFile>>();

            var fileId = Guid.NewGuid();

            await Assert.ThrowsAsync<Exception>(async () => await client.UploadFileAsync(fileId, new _fakes.FakeRequiredFile
            { }, stream));
        }

        #endregion
    }
}