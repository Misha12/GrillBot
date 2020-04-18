using System;
using System.Runtime.Serialization;

namespace Grillbot.Exceptions
{
    [Serializable]
    public class BotCommandInfoException : Exception
    {
        public BotCommandInfoException()
        {
        }

        public BotCommandInfoException(string message) : base(message)
        {
        }

        public BotCommandInfoException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BotCommandInfoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
