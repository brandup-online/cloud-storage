using BrandUp.CloudStorage.AwsS3.Tests._fakes;
using BrandUp.CloudStorage.Client;
using BrandUp.CloudStorage.Models.Interfaces;
using BrandUp.CloudStorage.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.CloudStorage.AwsS3.Tests
{
    public class AwsCloudClientTest : CloudStorageTestBase
    {
        readonly ICloudStorage storage;

        public AwsCloudClientTest()
        {
            storage = Services.GetRequiredService<ICloudStorage>();
        }

        [Fact]
        public Task Success_FromDI()
        {
            using var stream = new MemoryStream(Properties.Resources.Image);
            using var client = Services.GetRequiredService<ICloudClient<FakeFile>>();

            Assert.NotNull(client);

            return Task.CompletedTask;
            //await DoCRUD(client, new FakeFile { FakeGuid = Guid.NewGuid(), FakeString = "string" }, stream);
        }

        [Fact]
        public Task Success_FromStorage()
        {
            using var stream = new MemoryStream(Properties.Resources.Image);
            using var client = storage.CreateClient<FakeFile>();

            Assert.NotNull(client);

            return Task.CompletedTask;
            //await DoCRUD(client, new FakeFile { FakeGuid = Guid.NewGuid(), FakeString = "string" }, stream);
        }

        async Task DoCRUD<T>(ICloudClient<T> client, T metadata, Stream stream) where T : class, IFileMetadata, new()
        {
            var fileinfo = await client.UploadFileAsync(metadata, stream, CancellationToken.None);
            Assert.NotNull(fileinfo);

            var getFileinfo = await client.GetFileInfoAsync(fileinfo.FileId, CancellationToken.None);
            Assert.NotNull(getFileinfo);
            Assert.Equal(stream.Length, getFileinfo.Size);

            using var downlodadedStream = await client.ReadFileAsync(fileinfo.FileId, CancellationToken.None);
            Assert.NotNull(downlodadedStream);
            Assert.Equal(stream.Length, downlodadedStream.Length);

            var isDeleted = await client.DeleteFileAsync(fileinfo.FileId, CancellationToken.None);
            Assert.True(isDeleted);
        }
    }
}