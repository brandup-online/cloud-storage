namespace BrandUp.FileStorage
{
    public class ContextConfiguration
    {
        private IDictionary<string, string> configuration = new Dictionary<string, string>();

        public void AddCollection(string collectionKey, string colectionName)
          => configuration.TryAdd(collectionKey, colectionName);

        public bool TryGetConfiguration(string collectionKey, out string colectionName)
           => configuration.TryGetValue(collectionKey, out colectionName);
    }
}
