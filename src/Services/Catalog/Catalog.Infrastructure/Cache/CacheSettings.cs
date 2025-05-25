namespace Catalog.Infrastructure.Cache
{
    public class CacheSettings
    {
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
        public int AbsoluteExpirationInMinutes { get; set; }
        public int SlidingExpirationInMinutes { get; set; }
    }
}
