using System;

namespace Edit
{
    public class ConcurrencyException : Exception
    {
        public string StreamName { get; private set; }
        public IVersion ExpectedVersion { get; private set; }

        public ConcurrencyException(string streamName, IVersion expectedVersion)
            : base(string.Format("Version [{0}] is no longer current in stream [{1}].", expectedVersion, streamName))
        {
            StreamName = streamName;
            ExpectedVersion = expectedVersion;
        }
    }
}