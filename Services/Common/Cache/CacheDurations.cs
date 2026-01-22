using System;

namespace pos_service.Services.Common.Cache
{
    public static class CacheDurations
    {
        // Map CacheExpiry enum to concrete TimeSpan values
        public static TimeSpan GetTimespan(CacheExpiry expiry)
        {
            return expiry switch
            {
                CacheExpiry.FiveMinutes    => TimeSpan.FromMinutes(5),
                CacheExpiry.TenMinutes     => TimeSpan.FromMinutes(10),
                CacheExpiry.FifteenMinutes => TimeSpan.FromMinutes(15),
                CacheExpiry.ThirtyMinutes  => TimeSpan.FromMinutes(30),
                CacheExpiry.OneHour        => TimeSpan.FromHours(1),
                CacheExpiry.TwoHours       => TimeSpan.FromHours(2),
                CacheExpiry.SixHours       => TimeSpan.FromHours(6),
                CacheExpiry.TwelveHours    => TimeSpan.FromHours(12),
                CacheExpiry.OneDay         => TimeSpan.FromDays(1),
                CacheExpiry.ThreeDays      => TimeSpan.FromDays(3),
                _                          => TimeSpan.FromHours(1),
            };
        }
    }
}
