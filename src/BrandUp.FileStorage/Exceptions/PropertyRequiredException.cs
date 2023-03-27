namespace BrandUp.FileStorage.Exceptions
{
    public class PropertyRequiredException : Exception
    {
        public PropertyRequiredException(string propertyName) : base($"Property {propertyName} is required.") { }

    }
}
