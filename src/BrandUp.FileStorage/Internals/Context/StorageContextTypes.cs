namespace BrandUp.FileStorage.Internals.Context
{
    internal static class StorageContextTypes
    {
        readonly static Dictionary<Type, StorageContextInfo> contextsInfo = new();

        public static void RegisterContextType<TContext>()
            where TContext : FileStorageContext
        {
            var contextType = typeof(TContext);
            if (contextsInfo.ContainsKey(contextType))
                return;

            contextsInfo.Add(contextType, StorageContextInfo.Create<TContext>());
        }

        public static StorageContextInfo GetContextInfo<TContext>()
            where TContext : FileStorageContext
        {
            var contextType = typeof(TContext);
            if (!contextsInfo.TryGetValue(contextType, out var storageContextTypeMetadata))
                throw new ArgumentException();

            return storageContextTypeMetadata;
        }
    }
}