using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;

namespace OxDb.ServerCore.Logalytics.Utils
{
    public static class ActivityUtils
    {
        public static void SafeAddTag(Activity activity, string key, object? val)
        {
            if (activity != null && !string.IsNullOrEmpty(key) && val != null)
            {
                activity.SetTag(key, val);
            }
        }
    }
}
