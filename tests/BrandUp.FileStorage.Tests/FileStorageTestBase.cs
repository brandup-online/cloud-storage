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
        readonly IConfiguration config;
        readonly TestFileContext testFileContext;


        readonly protected byte[] image = Tests.Properties.Resources.Image;

        public IServiceProvider RootServices => rootServiceProvider;
        public IServiceProvider Services => serviceScope.ServiceProvider;
        public IConfiguration Configuration => config;

        public FileStorageTestBase()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var configBuilder = new ConfigurationBuilder()
              .AddUserSecrets(typeof(FileStorageTestBase).Assembly)
              .AddJsonFile("appsettings.test.json", true)
              .AddEnvironmentVariables();

            OnConfigurationBuilding(configBuilder);

            config = configBuilder.Build();

            var builder = services.AddFileStorage();

            OnConfigure(services, builder);

            rootServiceProvider = services.BuildServiceProvider();
            serviceScope = rootServiceProvider.CreateScope();

            testFileContext = Services.GetRequiredService<TestFileContext>();
        }

        static FileStorageTestBase()
        {
            TestGuid = Guid.Parse("ea3dd067-8bd8-4792-bfb0-e7be02d912e1");
        }

        #region Asserts 

        [Fact]

        public void Init_Success()
        {
            Assert.NotNull(testFileContext);
            Assert.NotNull(testFileContext.StorageProvider);

            var tempFiles = testFileContext.TempFiles;
            Assert.NotNull(tempFiles);
        }

        #endregion

        #region Test helpers


        protected async Task DoCRUD(TestFile file, Stream stream)
        {
            var fileinfo = await TestUploadAsync(file, stream);

            var getFileinfo = await TestGetAsync(fileinfo.Id);

            Equivalent(fileinfo.Metadata, getFileinfo.Metadata);

            await TestReadAsync(fileinfo.Id, stream);

            await TestDeleteAsync(fileinfo.Id);
        }

        internal async Task<File<TestFile>> TestUploadAsync(TestFile file, Stream stream)
        {
            var fileinfo = await testFileContext.TempFiles.UploadFileAsync(Guid.NewGuid(), file, stream, CancellationToken.None);
            Assert.NotNull(fileinfo);
            Assert.NotEqual(fileinfo.Id, default);
            Assert.Equal(fileinfo.Size, stream.Length);
            Equivalent(file, fileinfo.Metadata);

            return fileinfo;
        }

        internal async Task<File<TestFile>> TestGetAsync(Guid id)
        {
            var getFileinfo = await testFileContext.TempFiles.FindFileAsync(id, CancellationToken.None);
            Assert.NotNull(getFileinfo);
            Assert.Equal(getFileinfo.Id, id);
            Assert.True(getFileinfo.Size > 0);

            return getFileinfo;
        }

        internal async Task TestReadAsync(Guid id, Stream stream)
        {
            using var downlodadedStream = await testFileContext.TempFiles.ReadFileAsync(id, CancellationToken.None);
            Assert.NotNull(downlodadedStream);
            CompareStreams(stream, downlodadedStream);
        }

        internal async Task TestDeleteAsync(Guid id)
        {
            var isDeleted = await testFileContext.TempFiles.DeleteFileAsync(id, CancellationToken.None);
            Assert.True(isDeleted);

            Assert.Null(await testFileContext.TempFiles.FindFileAsync(id, CancellationToken.None));
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
