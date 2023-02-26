using BrandUp.FileStorage.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage
{
    public abstract class FileStorageTestBase : IAsyncLifetime
    {
        public static Guid TestGuid;

        readonly ServiceProvider rootServiceProvider;
        readonly IServiceScope serviceScope;
        readonly IConfiguration configuration;

        readonly public static byte[] image = Tests.Properties.Resources.Image;

        public IServiceProvider RootServices => rootServiceProvider;
        public IServiceProvider Services => serviceScope.ServiceProvider;
        public IConfiguration Configuration => configuration;

        public FileStorageTestBase()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var configBuilder = new ConfigurationBuilder()
              .AddUserSecrets(typeof(FileStorageTestBase).Assembly)
              .AddJsonFile("appsettings.test.json", true)
              .AddEnvironmentVariables();

            OnConfigurationBuilding(configBuilder);

            configuration = configBuilder.Build();

            var builder = services.AddFileStorage();

            OnConfigure(services, builder);

            rootServiceProvider = services.BuildServiceProvider();
            serviceScope = rootServiceProvider.CreateScope();
        }

        static FileStorageTestBase()
        {
            TestGuid = Guid.Parse("ea3dd067-8bd8-4792-bfb0-e7be02d912e1");
        }

        #region Helpers

        protected async Task CRUD(IFileCollection<TestFile> collection, TestFile file, Stream stream)
        {
            var fileinfo = await TestUploadAsync(collection, file, stream);
            EqualsAssert(file, fileinfo.Metadata);

            var getFileinfo = await TestGetAsync(collection, fileinfo.Id);
            EqualsAssert(file, getFileinfo.Metadata);

            await TestReadAsync(collection, fileinfo.Id, stream);

            await TestDeleteAsync(collection, fileinfo.Id);
        }


        protected async Task<File<T>> TestUploadAsync<T>(IFileCollection<T> fileCollection, T file, Stream stream) where T : class, new()
        {
            var id = Guid.NewGuid();
            var fileinfo = await fileCollection.UploadFileAsync(id, file, stream, CancellationToken.None);
            Assert.NotNull(fileinfo);
            Assert.Equal(fileinfo.Id, id);
            Assert.Equal(fileinfo.Size, stream.Length);

            return fileinfo;
        }

        protected async Task<File<T>> TestGetAsync<T>(IFileCollection<T> fileCollection, Guid id) where T : class, new()
        {
            var getFileinfo = await fileCollection.FindFileAsync(id, CancellationToken.None);
            Assert.NotNull(getFileinfo);
            Assert.Equal(getFileinfo.Id, id);
            Assert.True(getFileinfo.Size > 0);

            return getFileinfo;
        }

        protected async Task TestReadAsync<T>(IFileCollection<T> fileCollection, Guid id, Stream stream) where T : class, new()
        {
            using var downlodadedStream = await fileCollection.ReadFileAsync(id, CancellationToken.None);
            Assert.NotNull(downlodadedStream);
            CompareStreams(stream, downlodadedStream);
        }

        protected async Task TestDeleteAsync<T>(IFileCollection<T> fileCollection, Guid id) where T : class, new()
        {
            var isDeleted = await fileCollection.DeleteFileAsync(id, CancellationToken.None);
            Assert.True(isDeleted);

            Assert.Null(await fileCollection.FindFileAsync(id, CancellationToken.None));
        }

        #endregion

        #region Utils helpers

        static void CompareStreams(Stream expected, Stream actual)
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

        #region Equlals 

        protected void EqualsAssert(TestFile first, TestFile second)
        {
            Assert.NotNull(first);
            Assert.NotNull(second);

            Assert.Equal(first.FileName, second.FileName);
            Assert.Equal(first.Id, second.Id);
            Assert.Equal(first.Size, second.Size);
            Assert.Equal(first.CreatedDate, second.CreatedDate);
        }

        #endregion

        #region Virtual members

        protected virtual void OnConfigurationBuilding(IConfigurationBuilder builder) { }
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
        }

        #endregion
    }
}
