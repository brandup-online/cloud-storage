using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage
{
    public abstract class FileStorageTestBase<T> : IAsyncLifetime where T : class, IFileMetadata, new()
    {
        public static Guid TestGuid;

        readonly ServiceProvider rootServiceProvider;
        readonly IServiceScope serviceScope;
        readonly IConfiguration config;
        readonly IFileCollection<T> fileStorage;

        readonly protected byte[] image = Tests.Properties.Resources.Image;

        public IServiceProvider RootServices => rootServiceProvider;
        public IServiceProvider Services => serviceScope.ServiceProvider;
        public IConfiguration Configuration => config;
        public IFileCollection<T> Client => fileStorage;

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

            fileStorage = serviceScope.ServiceProvider.GetRequiredService<IFileCollection<T>>();
        }

        static FileStorageTestBase()
        {
            TestGuid = Guid.Parse("ea3dd067-8bd8-4792-bfb0-e7be02d912e1");
        }

        #region Test helpers

        internal abstract T CreateMetadataValue();

        protected async Task DoCRUD(T metadata, Stream stream)
        {
            var fileinfo = await TestUploadAsync(metadata, stream);

            var getFileinfo = await TestGetAsync(fileinfo.Id);

            Equivalent(fileinfo.Metadata, getFileinfo.Metadata);

            await TestReadAsync(fileinfo.Id, stream);

            await TestDeleteAsync(fileinfo.Id);
        }

        internal async Task<File<T>> TestUploadAsync(T metadata, Stream stream)
        {
            var fileinfo = await Client.UploadFileAsync(metadata, stream, CancellationToken.None);
            Assert.NotNull(fileinfo);
            Assert.NotEqual(fileinfo.FileId, default);
            Assert.Equal(fileinfo.Size, stream.Length);
            Equivalent(metadata, fileinfo.Metadata);

            return fileinfo;
        }

        internal async Task<File<T>> TestGetAsync(Guid id)
        {
            var getFileinfo = await Client.FindFileAsync(id, CancellationToken.None);
            Assert.NotNull(getFileinfo);
            Assert.Equal(getFileinfo.Id, id);
            Assert.True(getFileinfo.Size > 0);

            return getFileinfo;
        }

        internal async Task TestReadAsync(Guid id, Stream stream)
        {
            using var downlodadedStream = await Client.ReadFileAsync(id, CancellationToken.None);
            Assert.NotNull(downlodadedStream);
            CompareStreams(stream, downlodadedStream);
        }

        internal async Task TestDeleteAsync(Guid id)
        {
            var isDeleted = await Client.DeleteFileAsync(id, CancellationToken.None);
            Assert.True(isDeleted);

            Assert.Null(await Client.FindFileAsync(id, CancellationToken.None));
        }

        #endregion

        #region Utils helpers

        void Equivalent(object expected, object actual)
        {
            foreach (var property in expected.GetType().GetProperties())
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
