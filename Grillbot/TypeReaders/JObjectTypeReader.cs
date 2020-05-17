using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Grillbot.TypeReaders
{
    public class JObjectTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            try
            {
                var json = JObject.Parse(input);
                return Task.FromResult(TypeReaderResult.FromSuccess(json));
            }
            catch (JsonReaderException)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Byl zadán neplatný JSON."));
            }
        }
    }
}
