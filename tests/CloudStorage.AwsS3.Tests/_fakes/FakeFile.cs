using BrandUp.CloudStorage.Models.Interfaces;

namespace BrandUp.CloudStorage.AwsS3.Tests._fakes
{
    public class FakeFile : IFileMetadata
    {
        public string FakeString { get; set; }
        public Guid FakeGuid { get; set; }
        public int FakeInt { get; set; }
        public DateTime FakeDateTime { get; set; }
        public TimeSpan FakeTimeSpan { get; set; }
        public bool FakeBool { get; set; }
    }
}
