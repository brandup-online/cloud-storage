using BrandUp.FileStorage.AwsS3.Tests._fakes;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.AwsS3.Tests
{
    public class AwsCloudClientTest : CloudStorageTestBase
    {
        readonly ICloudClientFactory storage;

        public AwsCloudClientTest()
        {
            storage = Services.GetRequiredService<ICloudClientFactory>();
        }

        [Fact]
        public async Task Success_FromDI()
        {
            using var stream = new MemoryStream(Properties.Resources.Image);
            using var client = Services.GetRequiredService<ICloudClient<FakeFile>>();

            Assert.NotNull(client);

            await DoCRUD(client, new FakeFile { FakeGuid = Guid.NewGuid(), FakeString = "string" }, stream);
        }

        [Fact]
        public async Task Success_FromStorage()
        {
            using var stream = new MemoryStream(Properties.Resources.Image);
            using var client = storage.CreateClient<FakeFile>();

            Assert.NotNull(client);

            await DoCRUD(client, new FakeFile { FakeGuid = Guid.NewGuid(), FakeString = "string" }, stream);
        }

        async Task DoCRUD<T>(ICloudClient<T> client, T metadata, Stream stream) where T : class, new()
        {
            var fileinfo = await client.UploadFileAsync(metadata, stream, CancellationToken.None);
            Assert.NotNull(fileinfo);

            var getFileinfo = await client.GetFileInfoAsync(fileinfo.FileId, CancellationToken.None);
            Assert.NotNull(getFileinfo);

            var inputMetadata = metadata as FakeFile;
            var downloadedMetadata = getFileinfo.Metadata as FakeFile;
            Assert.Equal(inputMetadata.FakeBool, downloadedMetadata.FakeBool);
            Assert.Equal(inputMetadata.FakeString, downloadedMetadata.FakeString);
            Assert.Equal(inputMetadata.FakeDateTime, downloadedMetadata.FakeDateTime);
            Assert.Equal(inputMetadata.FakeInt, downloadedMetadata.FakeInt);
            Assert.Equal(inputMetadata.FakeTimeSpan, downloadedMetadata.FakeTimeSpan);
            Assert.Equal(inputMetadata.FakeGuid, downloadedMetadata.FakeGuid);

            Assert.Equal(stream.Length, getFileinfo.Size);

            using var downlodadedStream = await client.ReadFileAsync(fileinfo.FileId, CancellationToken.None);
            Assert.NotNull(downlodadedStream);
            Assert.Equal(stream.Length, downlodadedStream.Length);

            var isDeleted = await client.DeleteFileAsync(fileinfo.FileId, CancellationToken.None);
            Assert.True(isDeleted);
        }
    }
}