using BrandUp.FileStorage.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage
{
    public abstract class FileStorageTestBase : IAsyncLifetime
    {
        readonly ServiceProvider rootServiceProvider;
        readonly IServiceScope serviceScope;
        readonly IConfiguration config;

        public IServiceProvider RootServices => rootServiceProvider;
        public IServiceProvider Services => serviceScope.ServiceProvider;
        public IConfiguration Configuration => config;

        public FileStorageTestBase()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            config = new ConfigurationBuilder()
              .AddUserSecrets(typeof(FileStorageTestBase).Assembly)
              .AddJsonFile("appsettings.test.json", true)
              .AddEnvironmentVariables()
              .Build();

            var builder = services.AddFileStorage();

            OnConfigure(services, builder);

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

        #region Test helpers

        protected async Task DoCRUD<T>(IFileStorage<T> client, T metadata, Stream stream) where T : class, IFileMetadata, new()
        {
            var fileinfo = await TestUploadAsync(client, metadata, stream);

            var getFileinfo = await TestGetAsync(client, metadata, fileinfo.FileId, stream);

            await TestReadAsync(client, fileinfo.FileId, stream);

            await TestDeleteAsync(client, fileinfo.FileId);
        }

        protected async Task<IFileInfo<T>> TestUploadAsync<T>(IFileStorage<T> client, T metadata, Stream stream) where T : class, IFileMetadata, new()
        {
            var fileinfo = await client.UploadFileAsync(metadata, stream, CancellationToken.None);
            Assert.NotNull(fileinfo);
            Assert.NotEqual(fileinfo.FileId, default);
            Assert.Equal(fileinfo.Size, stream.Length);
            Equivalent(metadata, fileinfo.Metadata);

            return fileinfo;
        }

        protected async Task<IFileInfo<T>> TestGetAsync<T>(IFileStorage<T> client, T metadata, Guid id, Stream stream) where T : class, IFileMetadata, new()
        {
            var getFileinfo = await client.GetFileInfoAsync(id, CancellationToken.None);
            Assert.NotNull(getFileinfo);
            Assert.NotEqual(getFileinfo.FileId, default);
            Assert.Equal(getFileinfo.Size, stream.Length);
            Equivalent(metadata, getFileinfo.Metadata);

            return getFileinfo;
        }

        protected async Task TestReadAsync<T>(IFileStorage<T> client, Guid id, Stream stream) where T : class, IFileMetadata, new()
        {
            using var downlodadedStream = await client.ReadFileAsync(id, CancellationToken.None);
            Assert.NotNull(downlodadedStream);
            CompareStreams(stream, downlodadedStream);
        }

        protected async Task TestDeleteAsync<T>(IFileStorage<T> client, Guid id) where T : class, IFileMetadata, new()
        {
            var isDeleted = await client.DeleteFileAsync(id, CancellationToken.None);
            Assert.True(isDeleted);

            Assert.Null(await client.GetFileInfoAsync(id, CancellationToken.None));
        }

        #endregion

        #region Utils helpers

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
        #endregion

        #region Virtual members

        protected virtual void OnConfigure(IServiceCollection services, IFileStorageBuilder builder) { }
        protected virtual Task OnInitializeAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;
        protected virtual Task OnFinishAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;

        #endregion
    }
}
