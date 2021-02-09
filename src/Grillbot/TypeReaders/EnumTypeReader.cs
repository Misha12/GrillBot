using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Grillbot.TypeReaders
{
    public class EnumTypeReader<TEnum> : TypeReader where TEnum : struct
    {
        private Type EnumType { get; } = typeof(TEnum);
        public bool CaseSensitive { get; set; }

        public EnumTypeReader(bool caseSensitive)
        {
            CaseSensitive = caseSensitive;
        }

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (Enum.TryParse(input, !CaseSensitive, out TEnum result))
                return Task.FromResult(TypeReaderResult.FromSuccess(result));

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"Invalid value from enum {EnumType.Name}."));
        }
    }
}
