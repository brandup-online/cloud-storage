using BrandUp.FileStorage.AwsS3.Tests._fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.AwsS3.Tests
{
    public abstract class CloudStorageTestBase : IAsyncLifetime
    {
        readonly ServiceProvider rootServiceProvider;
        readonly IServiceScope serviceScope;

        public IServiceProvider RootServices => rootServiceProvider;
        public IServiceProvider Services => serviceScope.ServiceProvider;

        public CloudStorageTestBase()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var config = new ConfigurationBuilder()
               .AddUserSecrets(typeof(CloudStorageTestBase).Assembly)
               .AddJsonFile("appsettings.test.json", true)
               .AddEnvironmentVariables()
               .Build();

            var builder = services.AddFileStorage();

            builder.AddAwsS3Storage(config.GetSection("TestCloudStorage:Default"))
                    .AddAwsS3Bucket<FakeFile>(o => config.GetSection("TestCloudStorage:FakeFile").Bind(o));

            //builder.AddLocalStorage(config.GetSection("TestLocalStorage:Default"))
            //    .AddLocalFile<FakeFile1>()
            //    .AddLocalFile<FakeFile2>()
            //    .AddLocalFile<FakeFile3>();


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

        #region Virtual members

        protected virtual void OnConfigure(IServiceCollection services) { }
        protected virtual Task OnInitializeAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;
        protected virtual Task OnFinishAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;

        #endregion
    }
}
