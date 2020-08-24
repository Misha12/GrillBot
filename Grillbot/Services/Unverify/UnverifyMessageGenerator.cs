using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Unverify.Models;
using System;

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

        public string CreateUpdatePMMessage(SocketGuild guild, DateTime endDateTime)
        {
            var formatedEnd = endDateTime.ToLocaleDatetime();

            return $"Byl ti aktualizován čas pro odebrání práv na serveru **{guild.Name}**. Přístup ti bude navrácen **{formatedEnd}**.";
        }

        public string CreateUpdateChannelMessage(SocketGuildUser user, DateTime endDateTime)
        {
            var username = user.GetFullName();
            var formatedEnd = endDateTime.ToLocaleDatetime();

            return $"Reset konce odebrání přístupu pro uživatele **{username}** byl aktualizován.\nPřístup bude navrácen **{formatedEnd}**";
        }

        public string CreateRemoveAccessManuallyPMMessage(SocketGuild guild)
        {
            return $"Byl ti předčasně vrácen přístup na serveru **{guild.Name}**";
        }

        public string CreateRemoveAccessManuallyToChannel(SocketGuildUser user)
        {
            var username = user.GetFullName();

            return $"Předčasné vrácení přístupu pro uživatele **{username}** bylo dokončeno.";
        }

        public string CreateRemoveAccessManuallyFailed(SocketGuildUser user, Exception ex)
        {
            var username = user.GetFullName();
            var message = ex.GetFullMessage();

            return $"Předčasné vrácení přístupu pro uživatele **{username}** selhalo. ({message})";
        }

        public string CreateRemoveAccessUnverifyNotFound(SocketGuildUser user)
        {
            var username = user.GetFullName();

            return $"Předčasné vrácení přístupu pro uživatele **{username}** nelze provést. Unverify nebylo nalezeno.";
        }
    }
}
