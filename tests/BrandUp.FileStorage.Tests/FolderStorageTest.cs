using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Folder;
using BrandUp.FileStorage.Tests._fakes.Local;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests
{
    public class FolderStorageTest : FileStorageTestBase
    {
        readonly IFileStorageFactory factory;

        public FolderStorageTest()
        {
            factory = Services.GetRequiredService<IFileStorageFactory>();
        }

        [Fact]
        public async Task Success()
        {
            using var stream = new MemoryStream(Properties.Resources.Image);
            using var client = factory.CreateFolderStorage<FakeLocalFile>();

            Assert.NotNull(client);

            await DoCRUD(client, new FakeLocalFile
            {
                FakeInner = new() { FakeGuid = Guid.NewGuid(), FakeBool = true },
                FileName = "string.png",
                FakeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),// Default type converter have minute accuracy 
                FakeInt = 21332,
                FakeTimeSpan = TimeSpan.FromSeconds(127)
            }, stream);
        }

        async Task DoCRUD<T>(IFileStorage<T> client, T metadata, Stream stream) where T : class, IFileMetadata, new()
        {
            var fileinfo = await client.UploadFileAsync(metadata, stream, CancellationToken.None);
            Assert.NotNull(fileinfo);

            var getFileinfo = await client.GetFileInfoAsync(fileinfo.FileId, CancellationToken.None);
            Assert.NotNull(getFileinfo);

            var inputMetadata = metadata as FakeLocalFile;
            var downloadedMetadata = getFileinfo.Metadata as FakeLocalFile;

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