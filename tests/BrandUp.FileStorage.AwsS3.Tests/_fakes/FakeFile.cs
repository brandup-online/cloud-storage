namespace BrandUp.FileStorage.Tests._fakes
{
    public class FakeFile : IFileMetadata
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public int FakeInt { get; set; }
        public DateTime FakeDateTime { get; set; }
        public TimeSpan FakeTimeSpan { get; set; }
        public FakeInnnerClass FakeInnner { get; set; }

    }
    public class FakeInnnerClass
    {
        public bool FakeBool { get; set; }
        public Guid FakeGuid { get; set; }
    }
}
