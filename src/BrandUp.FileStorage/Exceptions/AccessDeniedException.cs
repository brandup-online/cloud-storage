namespace BrandUp.FileStorage.Exceptions
{
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(Exception innerException) : base("Access denied", innerException) { }
    }
}
