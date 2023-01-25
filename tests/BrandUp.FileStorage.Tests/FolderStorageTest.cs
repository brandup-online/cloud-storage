using BrandUp.FileStorage.Folder;
using BrandUp.FileStorage.Tests._fakes;
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

        [Fact]
        public async Task Success()
        {
            using var stream = new MemoryStream(Properties.Resources.Image);
            using var client = factory.CreateFolderStorage<FakeFile>();

            Assert.NotNull(client);

            await DoCRUD(client, new FakeFile
            {
                FakeInnner = new() { FakeGuid = Guid.NewGuid(), FakeBool = true },
                FileName = "string",
                Extension = "png",
                FakeDateTime = new DateTime(2002, 12, 15, 13, 45, 0),// Default type converter have accuracy to minute
                FakeInt = 21332,
                FakeTimeSpan = TimeSpan.FromSeconds(127)
            }, stream);
        }
    }
}