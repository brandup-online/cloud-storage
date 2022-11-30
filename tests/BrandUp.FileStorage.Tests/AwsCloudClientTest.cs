using BrandUp.FileStorage.AwsS3;
using BrandUp.FileStorage.Tests._fakes;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests
{
    public class AwsCloudClientTest : FileStorageTestBase
    {
        readonly IFileStorageFactory factory;

        public AwsCloudClientTest()
        {
            factory = Services.GetRequiredService<IFileStorageFactory>();
        }

        [Fact]
        public async Task Success_FromStorage()
        {
            using var stream = new MemoryStream(Properties.Resources.Image);
            using var client = factory.CreateAwsStorage<FakeFile>();

            Assert.NotNull(client);

            await DoCRUD(client, new FakeFile
            {
                FakeInnner = new() { FakeGuid = Guid.NewGuid(), FakeBool = true },
                FileName = "string",
                Extension = "png",
                FakeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),// У дефолтного конвертера типов точность минута
                FakeInt = 21332,
                FakeTimeSpan = TimeSpan.FromSeconds(127)
            }, stream);
        }
    }
}