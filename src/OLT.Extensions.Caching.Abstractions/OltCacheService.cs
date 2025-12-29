//namespace OLT.Core
//{
//    [Obsolete("Removing in 10.x, Move to IOltHybridCache")]
//    public abstract class OltCacheService : IOltCacheService
//    {
//        protected virtual string ToCacheKey(string key)
//        {
//            ArgumentNullException.ThrowIfNullOrEmpty(key);
//            return key.ToLower();
//        }

//        public abstract TEntry Get<TEntry>(string key, Func<TEntry> factory, TimeSpan? absoluteExpiration = null);
//        public abstract Task<TEntry> GetAsync<TEntry>(string key, Func<Task<TEntry>> factory, TimeSpan? absoluteExpiration = null);
//        public abstract void Remove(string key);
//        public abstract Task RemoveAsync(string key);
//        public abstract bool Exists(string key);
//        public abstract Task<bool> ExistsAsync(string key);
//        public abstract void Flush();
//        public abstract Task FlushAsync();
//    }
//}