#region

using System;

#endregion

namespace M3U.NET.Extensions
{
    internal static class UriExtensions
    {
        public static Uri MakeAbsoluteUri(this Uri u, Uri uri)
        {
            return new Uri(u, uri);
        }
    }
}