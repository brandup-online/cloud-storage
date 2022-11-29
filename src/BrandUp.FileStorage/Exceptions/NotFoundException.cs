namespace BrandUp.FileStorage.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(Exception innerException) : base("File not found", innerException) { }
    }
}
