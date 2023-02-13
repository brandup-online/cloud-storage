using BrandUp.FileStorage.Attributes;

namespace BrandUp.FileStorage.Exceptions
{
    public class RequiredMetadataException : Exception
    {
        /// <summary>
        /// Throws if property attributed by <see cref="MetadataRequiredAttribute"/> but null
        /// </summary>
        public RequiredMetadataException(string propertyName) : base($"{propertyName} is required.") { }
    }
}
