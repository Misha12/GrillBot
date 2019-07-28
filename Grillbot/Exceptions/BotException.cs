using Discord.Commands;
using System;
using System.Runtime.Serialization;

namespace Grillbot.Exceptions
{
    [Serializable]
    public class BotException : Exception
    {
        public IResult Result { get; set; }

        public BotException()
        {
        }

        public BotException(IResult result)
        {
            Result = result;
        }

        public BotException(string message) : base(message)
        {
        }

        public BotException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BotException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string ToString() => Result.ToString() + Environment.NewLine + base.ToString();
    }
}
