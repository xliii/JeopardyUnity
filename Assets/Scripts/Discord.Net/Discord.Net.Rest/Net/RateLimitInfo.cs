using System;
using System.Collections.Generic;

namespace Discord.Net
{
    internal struct RateLimitInfo
    {
        public bool IsGlobal { get; }
        public int? Limit { get; }
        public int? Remaining { get; }
        public int? RetryAfter { get; }
        public DateTimeOffset? Reset { get; }
        public TimeSpan? Lag { get; }

        internal RateLimitInfo(Dictionary<string, string> headers)
        {
            string temp;
            bool isGlobal;
            int limit;
            int remaining;
            int reset;
            int retryAfter;
            DateTimeOffset date;
            IsGlobal = headers.TryGetValue("X-RateLimit-Global", out temp) &&
                       bool.TryParse(temp, out isGlobal) && isGlobal;
            Limit = headers.TryGetValue("X-RateLimit-Limit", out temp) && 
                int.TryParse(temp, out limit) ? limit : (int?)null;
            Remaining = headers.TryGetValue("X-RateLimit-Remaining", out temp) && 
                int.TryParse(temp, out remaining) ? remaining : (int?)null;
            Reset = headers.TryGetValue("X-RateLimit-Reset", out temp) && 
                int.TryParse(temp, out reset) ? DateTimeOffset.FromUnixTimeSeconds(reset) : (DateTimeOffset?)null;
            RetryAfter = headers.TryGetValue("Retry-After", out temp) &&
                int.TryParse(temp, out retryAfter) ? retryAfter : (int?)null;
            Lag = headers.TryGetValue("Date", out temp) &&
                DateTimeOffset.TryParse(temp, out date) ? DateTimeOffset.UtcNow - date : (TimeSpan?)null;
        }
    }
}
