using System;
using System.Runtime.Serialization;

namespace Grillbot.Exceptions
{
    [Serializable]
    public class BadRequestException : Exception
    {
        public BadRequestException()
        {
        }

        public BadRequestException(string message, object data) : base(message)
        {
            Data.Add("Data", data);
        }

        public BadRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BadRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public BadRequestException(string message) : base(message)
        {
        }
    }
}
