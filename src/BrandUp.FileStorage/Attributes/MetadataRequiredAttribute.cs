using System.ComponentModel.DataAnnotations;

namespace BrandUp.FileStorage.Attributes
{
    public class MetadataRequiredAttribute : RequiredAttribute
    {
        public MetadataRequiredAttribute()
        {
            AllowEmptyStrings = false;
        }
    }
}
