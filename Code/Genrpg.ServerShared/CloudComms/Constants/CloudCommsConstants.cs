using System;

namespace Genrpg.ServerShared.CloudComms.Constants
{
    public class CloudCommsConstants
    {
        public static readonly TimeSpan EndpointDeleteTime = TimeSpan.FromDays(7);
        public static readonly TimeSpan MessageDeleteTime = TimeSpan.FromSeconds(10);

        public static double MessageTtlSeconds = 5.0f;
    }
}


