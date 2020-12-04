using System;
using System.Runtime.Serialization;

namespace Grillbot.Exceptions
{
    [Serializable]
    public class ThrowHelpException : Exception
    {
        public ThrowHelpException()
        {
        }

        public ThrowHelpException(string message) : base(message)
        {
        }

        public ThrowHelpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ThrowHelpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
