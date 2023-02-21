using BrandUp.MongoDB;

namespace BrandUp.FileStorage.MongoDb
{

    public class TestMongoContext : MongoDbContext
    {
        public TestMongoContext(MongoDbContextOptions options) : base(options)
        {
        }
    }
}
