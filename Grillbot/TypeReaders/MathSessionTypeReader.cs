using Discord.Commands;
using Grillbot.Services.Math;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.TypeReaders
{
    public class MathSessionTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (!int.TryParse(input, out int id))
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "ID nelze převést na INT"));

            using var scope = services.CreateScope();
            var service = scope.ServiceProvider.GetService<MathService>();
            var session = service.Sessions.Find(o => o.ID == id);

            if (session == null)
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Session nebyla nalezena."));

            return Task.FromResult(TypeReaderResult.FromSuccess(session));
        }
    }
}
