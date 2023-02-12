using BrandUp.FileStorage.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage
{
    public abstract class FileStorageTestBase<T> : IAsyncLifetime where T : class, IFileMetadata, new()
    {
        readonly ServiceProvider rootServiceProvider;
        readonly IServiceScope serviceScope;
        readonly IConfiguration config;
        readonly IFileStorage<T> fileStorage;

        readonly byte[] image = Tests.Properties.Resources.Image;

        public IServiceProvider RootServices => rootServiceProvider;
        public IServiceProvider Services => serviceScope.ServiceProvider;
        public IConfiguration Configuration => config;
        public IFileStorage<T> Client => fileStorage;

        public FileStorageTestBase()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            config = new ConfigurationBuilder()
              .AddUserSecrets(typeof(FileStorageTestBase<>).Assembly)
              .AddJsonFile("appsettings.test.json", true)
              .AddEnvironmentVariables()
              .Build();

            var builder = services.AddFileStorage();

            OnConfigure(services, builder);

            rootServiceProvider = services.BuildServiceProvider();
            serviceScope = rootServiceProvider.CreateScope();

            fileStorage = serviceScope.ServiceProvider.GetService<IFileStorage<T>>();
        }

        #region Asserts

        /// <summary>
        /// Scope returns only one instance of storage 
        /// </summary>
        [Fact]
        public void Succses_SameInstanses()
        {
            using var storage1 = Services.GetRequiredService<IFileStorage<T>>();
            using var storage2 = Services.GetRequiredService<IFileStorage<T>>();

            Assert.Same(storage1, storage2);
        }

        [Fact]
        public void Succses_SDifferentScopes()
        {
            using var scope1 = Services.CreateScope();
            using var storage1 = scope1.ServiceProvider.GetRequiredService<IFileStorage<T>>();

            using var scope2 = Services.CreateScope();
            using var storage2 = scope2.ServiceProvider.GetRequiredService<IFileStorage<T>>();

            Assert.NotSame(storage1, storage2);
        }

        [Fact]
        public async Task Success_CRUD()
        {
            using var stream = new MemoryStream(image);
            var metadata = CreateMetadataValue();

            await DoCRUD(metadata, stream);
        }

        #endregion

        #region Test helpers

        protected abstract T CreateMetadataValue();

        protected async Task DoCRUD(T metadata, Stream stream)
        {
            var fileinfo = await TestUploadAsync(Client, metadata, stream);

            var getFileinfo = await TestGetAsync(Client, metadata, fileinfo.FileId, stream);

            await TestReadAsync(Client, fileinfo.FileId, stream);

            await TestDeleteAsync(Client, fileinfo.FileId);
        }

        protected async Task<IFileInfo<T>> TestUploadAsync(IFileStorage<T> client, T metadata, Stream stream)
        {
            var fileinfo = await client.UploadFileAsync(metadata, stream, CancellationToken.None);
            Assert.NotNull(fileinfo);
            Assert.NotEqual(fileinfo.FileId, default);
            Assert.Equal(fileinfo.Size, stream.Length);
            Equivalent(metadata, fileinfo.Metadata);

            return fileinfo;
        }

        protected async Task<IFileInfo<T>> TestGetAsync(IFileStorage<T> client, T metadata, Guid id, Stream stream)
        {
            var getFileinfo = await client.GetFileInfoAsync(id, CancellationToken.None);
            Assert.NotNull(getFileinfo);
            Assert.NotEqual(getFileinfo.FileId, default);
            Assert.Equal(getFileinfo.Size, stream.Length);
            Equivalent(metadata, getFileinfo.Metadata);

            return getFileinfo;
        }

        protected async Task TestReadAsync(IFileStorage<T> client, Guid id, Stream stream)
        {
            using var downlodadedStream = await client.ReadFileAsync(id, CancellationToken.None);
            Assert.NotNull(downlodadedStream);
            CompareStreams(stream, downlodadedStream);
        }

        protected async Task TestDeleteAsync(IFileStorage<T> client, Guid id)
        {
            var isDeleted = await client.DeleteFileAsync(id, CancellationToken.None);
            Assert.True(isDeleted);

            Assert.Null(await client.GetFileInfoAsync(id, CancellationToken.None));
        }

        #endregion

        #region Utils helpers

        void Equivalent(T expected, T actual)
        {
            foreach (var property in typeof(T).GetProperties())
            {
                var expectedValue = (T)property.GetValue(expected, null);
                var actualValue = (T)property.GetValue(actual, null);

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
            Client.Dispose();
        }

        #endregion
    }
}
