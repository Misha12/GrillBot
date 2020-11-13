using Discord.Commands;
using Grillbot.Enums;
using System;
using System.Threading.Tasks;

namespace Grillbot.TypeReaders
{
    public class GlobalConfigItemTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (Enum.TryParse(input, out GlobalConfigItems result))
                return Task.FromResult(TypeReaderResult.FromSuccess(result));

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid key from GlobalConfigItems. Get available keys from `globalConfig keys` command."));
        }
    }
}
