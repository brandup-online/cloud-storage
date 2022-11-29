namespace BrandUp.FileStorage.Folder.Tests._fakes
{
    public class FakeFile : IFileMetadata
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public Guid FakeGuid { get; set; }
        public int FakeInt { get; set; }
        public DateTime FakeDateTime { get; set; }
        public TimeSpan FakeTimeSpan { get; set; }
        public bool FakeBool { get; set; }


    }
}
