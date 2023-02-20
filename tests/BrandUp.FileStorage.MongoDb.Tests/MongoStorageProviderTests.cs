using BrandUp.FileStorage.Builder;
using BrandUp.MongoDB;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.MongoDb.Tests
{
    public class MongoStorageProviderTests : FileStorageTestBase
    {
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {

            services.AddMongoDbContext<TestMongoContext>(builder =>
            {
                builder.DatabaseName = "Test";
                builder.UseCamelCaseElementName();
                builder.UseIgnoreIfNull(true);
            });

            services.AddMongo2GoDbClientFactory();

            builder.AddMongoProvider<TestMongoContext>("mongo", options => { });

            services.AddFileContext<TestFileContext>("mongo");
        }
    }

}