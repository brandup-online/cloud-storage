using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.Tests._fakes.Local;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests
{
    public class FolderStorageTest : FileStorageTestBase
    {
        readonly IFileStorageFactory factory;

        public FolderStorageTest()
        {
            factory = Services.GetRequiredService<IFileStorageFactory>();
        }

        /// <summary>
        /// Default functionality test.
        /// </summary>
        [Fact]
        public async Task Success()
        {
            using var stream = new MemoryStream(Properties.Resources.Image);
            using var client = factory.Create<FakeLocalFile>();

            Assert.NotNull(client);

            await DoCRUD(client, new FakeLocalFile
            {
                FakeInner = new() { FakeGuid = Guid.NewGuid(), FakeBool = true },
                FileName = "string.png",
                FakeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),// Default type converter have minute accuracy 
                FakeInt = 21332,
                FakeTimeSpan = TimeSpan.FromSeconds(127)
            }, stream);
        }
    }
}