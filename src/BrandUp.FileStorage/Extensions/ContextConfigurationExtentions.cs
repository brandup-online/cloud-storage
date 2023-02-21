using Microsoft.Extensions.Configuration;

namespace BrandUp.FileStorage.Extensions
{
    public static class ContextConfigurationExtentions
    {
        public static void FromConfiguration(this ContextConfiguration configuration, IConfiguration configurationSection)
        {
            foreach (var pair in configurationSection.AsEnumerable())
                if (pair.Value != null)
                    configuration.AddCollection(pair.Key.Split(':').Last(), pair.Value);
        }
    }
}
