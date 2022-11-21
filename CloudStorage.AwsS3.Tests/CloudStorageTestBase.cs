using CloudStorage.AwsS3.Extensions;
using CloudStorage.AwsS3.Tests._fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudStorage.AwsS3.Tests
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

            var myConfiguration = new Dictionary<string, string>()
            {
                {"Default:ServiceUrl", "***Your Link***"},
                {"Default:AuthenticationRegion", "***Your Region***"},
                {"Default:AccessKeyId", "***Your Key***"},
                {"Default:SecretAccessKey", "***Your Secret***"},

                {"FakeFile:BucketName", "***Your Bucket***"},
            };

            var configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(myConfiguration)
                            .Build();

            services.AddAwsS3Builder()
                .AddAwsCloudStorage(configuration)
                .AddClient<FakeFile>();

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
