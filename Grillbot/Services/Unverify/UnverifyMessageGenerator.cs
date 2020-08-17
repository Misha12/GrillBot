using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Unverify.Models;

namespace Grillbot.Services.Unverify
{
    public class UnverifyMessageGenerator
    {
        public string CreateUnverifyMessageToChannel(UnverifyUserProfile profile)
        {
            var endDateTime = profile.EndDateTime.ToLocaleDatetime();
            var username = profile.DestinationUser.GetFullName();

            return $"Dočasné odebrání přístupu pro uživatele **{username}** bylo dokončeno. Přístup bude navrácen **{endDateTime}**. Důvod: {profile.Reason}";
        }

        public string CreateUnverifyPMMessage(UnverifyUserProfile profile, SocketGuild guild)
        {
            var endDateTime = profile.EndDateTime.ToLocaleDatetime();

            return $"Byly ti dočasně odebrány všechny práva na serveru **{guild.Name}**. Přístup ti bude navrácen **{endDateTime}**. Důvod: {profile.Reason}";
        }
    }
}
