using System;
using System.Runtime.Serialization;

namespace WatchDog_Bot.Exceptions
{
    [Serializable]
    public class ConfigException : Exception
    {
        public ConfigException()
        {
        }

        public ConfigException(string message) : base(message)
        {
        }

        public ConfigException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfigException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
