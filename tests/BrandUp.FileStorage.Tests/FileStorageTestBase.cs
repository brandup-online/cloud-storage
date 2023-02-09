using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.AwsS3;
using BrandUp.FileStorage.Folder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests
{
    public abstract class FileStorageTestBase : IAsyncLifetime
    {
        readonly ServiceProvider rootServiceProvider;
        readonly IServiceScope serviceScope;

        public IServiceProvider RootServices => rootServiceProvider;
        public IServiceProvider Services => serviceScope.ServiceProvider;

        public FileStorageTestBase()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var config = new ConfigurationBuilder()
               .AddUserSecrets(typeof(FileStorageTestBase).Assembly)
               .AddJsonFile("appsettings.test.json", true)
               .AddEnvironmentVariables()
               .Build();

            var builder = services.AddFileStorage();

            builder.AddAwsS3Storage(config.GetSection("TestCloudStorage"))
                    .AddAwsS3Bucket<_fakes.Aws.FakeAwsFile>()
                    .AddAwsS3Bucket<_fakes.Aws.AttributedFakeFile>("FakeAwsFile")
                    .AddAwsS3Bucket<_fakes.Aws.FakeMetadataOld>("FakeAwsFile")
                    .AddAwsS3Bucket<_fakes.Aws.FakeMetadataNew>("FakeAwsFile");

            builder.AddFolderStorage(config.GetSection("TestFolderStorage"))
                  .AddFolderFor<_fakes.Local.FakeLocalFile>();

            OnConfigure(services);

            rootServiceProvider = services.BuildServiceProvider();
            serviceScope = rootServiceProvider.CreateScope();
        }

        #region IAsyncLifetime region

        public async Task InitializeAsync()
        {
            await OnInitializeAsync(rootServiceProvider, serviceScope.ServiceProvider);
        }

        public async Task DisposeAsync()
        {
            await OnFinishAsync(rootServiceProvider, serviceScope.ServiceProvider);

            serviceScope.Dispose();
            await rootServiceProvider.DisposeAsync();
        }

        #endregion

        protected async Task DoCRUD<T>(IFileStorage<T> client, T metadata, Stream stream) where T : class, IFileMetadata, new()
        {
            var fileinfo = await client.UploadFileAsync(metadata, stream, CancellationToken.None);
            Assert.NotNull(fileinfo);
            Assert.NotEqual(fileinfo.FileId, default);
            Assert.Equal(fileinfo.Size, stream.Length);
            Equivalent(metadata, fileinfo.Metadata);

            var getFileinfo = await client.GetFileInfoAsync(fileinfo.FileId, CancellationToken.None);
            Assert.NotNull(getFileinfo);
            Assert.NotEqual(getFileinfo.FileId, default);
            Assert.Equal(getFileinfo.Size, stream.Length);
            Equivalent(metadata, getFileinfo.Metadata);

            using var downlodadedStream = await client.ReadFileAsync(fileinfo.FileId, CancellationToken.None);
            Assert.NotNull(downlodadedStream);
            CompareStreams(stream, downlodadedStream);

            var isDeleted = await client.DeleteFileAsync(fileinfo.FileId, CancellationToken.None);
            Assert.True(isDeleted);

            Assert.Null(await client.GetFileInfoAsync(fileinfo.FileId, CancellationToken.None));
        }

        void Equivalent<T>(T expected, T actual)
        {
            foreach (var property in typeof(T).GetProperties())
            {
                var expectedValue = property.GetValue(expected, null);
                var actualValue = property.GetValue(actual, null);

                if (property.PropertyType.IsSerializable)
                {
                    Assert.Equal(expectedValue, actualValue);
                }
                else
                {
                    Equivalent(expectedValue, actualValue);
                }
            }
        }

        void CompareStreams(Stream expected, Stream actual)
        {
            Stream assertStream = null;
            using var ms = new MemoryStream();
            try
            {
                actual.Seek(0, SeekOrigin.Begin);
                assertStream = actual;
            }
            catch
            {
                actual.CopyTo(ms);
                assertStream = ms;
                assertStream.Seek(0, SeekOrigin.Begin);
            }
            expected.Seek(0, SeekOrigin.Begin);

            Assert.Equal(expected.Length, actual.Length);

            var bytesToRead = sizeof(long);

            byte[] one = new byte[bytesToRead];
            byte[] two = new byte[bytesToRead];

            int iterations = (int)Math.Ceiling((double)expected.Length / bytesToRead);

            for (int i = 0; i < iterations; i++)
            {
                expected.Read(one, 0, bytesToRead);
                assertStream.Read(two, 0, bytesToRead);

                Assert.Equal(BitConverter.ToInt64(one, 0), BitConverter.ToInt64(two, 0));
            }

        }

        #region Virtual members

        protected virtual void OnConfigure(IServiceCollection services) { }
        protected virtual Task OnInitializeAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;
        protected virtual Task OnFinishAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;

        #endregion
    }
}
