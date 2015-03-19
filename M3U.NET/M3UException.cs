#region

using System;

#endregion

namespace M3U.NET
{
    public class M3UException : Exception
    {
        public M3UException()
        {
        }

        public M3UException(string message) : base(message)
        {
        }
    }
}