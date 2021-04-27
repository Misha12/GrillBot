using Discord.Commands;
using System.Threading.Tasks;
using Grillbot.Services.MemeImages;
using Grillbot.Attributes;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Extensions.Discord;
using Grillbot.Enums;
using System;
using System.Linq;
using Grillbot.Services.Duck;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Grillbot.Helpers;
using Grillbot.Services.MessageCache;
using Discord;
using Discord.WebSocket;
using System.IO;
using SysDrawImage = System.Drawing.Image;
using SysImgFormat = System.Drawing.Imaging.ImageFormat;
using Grillbot.Extensions;
using Grillbot.Resources.Peepolove;
using System.Drawing;
using GrapeCity.Documents.Imaging;
using Grillbot.Resources.Peepoangry;

namespace Grillbot.Modules
{
    [ModuleID("MemeModule")]
    [Name("Ostatní zbytečnosti")]
    public class MemeModule : BotModuleBase
    {
        public MemeModule(IServiceProvider provider) : base(provider: provider)
        {
            if (!Directory.Exists("ImageCache"))
                Directory.CreateDirectory("ImageCache");
        }

        [Command("nudes")]
        public async Task SendNudeAsync()
        {
            await SendAsync("nudes").ConfigureAwait(false);
        }

        [Command("notnudes")]
        public async Task SendNotNudesAsync()
        {
            await SendAsync("notnudes").ConfigureAwait(false);
        }

        private async Task SendAsync(string category)
        {
            using var service = GetService<MemeImagesService>();
            var content = await service.Service.GetRandomFileAsync(Context.Guild, category);

            if (content == null)
            {
                await ReplyAsync("Nemám žádný obrázek.");
                return;
            }

            await ReplyFileAsync(content, $"{category}.png");
        }

        #region Peepolove

        [Command("peepolove")]
        [Alias("love")]
        public async Task PeepoloveAsync(IUser member = null)
        {
            if (member == null) member = Context.User;
            var imageName = CreateCachePath($"Peepolove_{member.Id}_{member.AvatarId ?? member.Discriminator}.{(member.AvatarId?.StartsWith("a_") == true ? "gif" : "png")}");

            if (!File.Exists(imageName))
            {
                var profilePictureData = await member.DownloadAvatarAsync(256);
                using var memStream = new MemoryStream(profilePictureData);
                using var rawProfilePicture = SysDrawImage.FromStream(memStream);

                if (Path.GetExtension(imageName) == ".gif" && profilePictureData.Length > 2 * (Context.Guild.CalculateFileUploadLimit() / 3))
                    imageName = Path.ChangeExtension(imageName, ".png");

                if (Path.GetExtension(imageName) == ".gif")
                {
                    var frames = rawProfilePicture.SplitGifIntoFrames();

                    try
                    {
                        using var gifWriter = new GcGifWriter(imageName);
                        using var gcBitmap = new GcBitmap();

                        foreach (var userFrame in frames)
                        {
                            using var roundedUserFrame = userFrame.RoundImage();
                            using var frame = RenderPeepoloveFrame(roundedUserFrame);

                            using var ms = new MemoryStream();
                            frame.Save(ms, SysImgFormat.Png);

                            gcBitmap.Load(ms.ToArray());
                            gifWriter.AppendFrame(gcBitmap, disposalMethod: GifDisposalMethod.RestoreToBackgroundColor, delayTime: rawProfilePicture.CalculateGifDelay());
                        }
                    }
                    finally
                    {
                        frames.ForEach(o => o.Dispose());
                        frames.Clear();
                    }
                }
                else if (Path.GetExtension(imageName) == ".png")
                {
                    using var roundedProfileImage = rawProfilePicture.RoundImage();
                    using var profilePicture = roundedProfileImage.ResizeImage(256, 256);

                    using var frame = RenderPeepoloveFrame(profilePicture);
                    frame.Save(imageName, SysImgFormat.Png);
                }
            }

            await ReplyFileAsync(imageName);
        }

        static private SysDrawImage RenderPeepoloveFrame(SysDrawImage profilePicture)
        {
            using var body = new Bitmap(PeepoloveResources.body);
            using var graphics = Graphics.FromImage(body);

            graphics.RotateTransform(-0.4F);
            graphics.DrawImage(profilePicture, new Rectangle(5, 312, 180, 180));
            graphics.RotateTransform(0.4F);
            graphics.DrawImage(PeepoloveResources.hands, new Rectangle(0, 0, 512, 512));

            graphics.DrawImage(body, new Point(0, 0));
            return (body as SysDrawImage).CropImage(new Rectangle(0, 115, 512, 397));
        }

        #endregion

        #region Peepoangry

        [Command("peepoangry")]
        [Alias("peepoCantBelieveThisShit", "angry")]
        [Summary("PeepoAngry emote zírající na profilovku uživatele.")]
        public async Task PeepoAngryAsync(IUser member = null)
        {
            if (member == null) member = Context.User;
            var imageName = CreateCachePath($"Peepoangry_{member.Id}_{member.AvatarId ?? member.Discriminator}.{(member.AvatarId?.StartsWith("a_") == true ? "gif" : "png")}");

            if (!File.Exists(imageName))
            {
                var profilePictureData = await member.DownloadAvatarAsync(64);
                using var memStream = new MemoryStream(profilePictureData);
                using var rawProfilePicture = SysDrawImage.FromStream(memStream);

                if (Path.GetExtension(imageName) == ".gif" && profilePictureData.Length >= 2 * (Context.Guild.CalculateFileUploadLimit() / 3))
                    imageName = Path.ChangeExtension(imageName, ".png");

                if (Path.GetExtension(imageName) == ".gif")
                {
                    var frames = rawProfilePicture.SplitGifIntoFrames();

                    try
                    {
                        using var gifWriter = new GcGifWriter(imageName);
                        using var gcBitmap = new GcBitmap();

                        foreach (var userFrame in frames)
                        {
                            using var roundedUserFrame = userFrame.RoundImage();
                            using var frame = RenderPeepoangryFrame(roundedUserFrame);

                            using var ms = new MemoryStream();
                            frame.Save(ms, SysImgFormat.Png);

                            gcBitmap.Load(ms.ToArray());
                            gifWriter.AppendFrame(gcBitmap, disposalMethod: GifDisposalMethod.RestoreToBackgroundColor, delayTime: rawProfilePicture.CalculateGifDelay());
                        }
                    }
                    finally
                    {
                        frames.ForEach(o => o.Dispose());
                        frames.Clear();
                    }
                }
                else if (Path.GetExtension(imageName) == ".png")
                {
                    using var roundedProfileImage = rawProfilePicture.RoundImage();
                    var profilePicture = roundedProfileImage.ResizeImage(64, 64);

                    using var frame = RenderPeepoangryFrame(profilePicture);
                    frame.Save(imageName, SysImgFormat.Png);
                }
            }

            await ReplyFileAsync(imageName);
        }

        static private SysDrawImage RenderPeepoangryFrame(SysDrawImage profilePicture)
        {
            var body = new Bitmap(250, 105);
            using var graphics = Graphics.FromImage(body);

            graphics.DrawImage(profilePicture, new Rectangle(new Point(20, 10), new Size(64, 64)));
            graphics.DrawImage(PeepoangryResources.peepoangry, new Point(115, -5));

            return body;
        }

        #endregion

        [Command("grillhi"), Alias("hi")]
        public async Task GreetAsync()
        {
            await GreetAsync(null);
        }

        [Command("grillhi"), Alias("hi")]
        [Remarks("Možné formáty odpovědi jsou 'text', 'bin', nebo 'hex'.")]
        public async Task GreetAsync(string mode)
        {
            var config = await GetMethodConfigAsync<GreetingConfig>("", "grillhi");

            if (string.IsNullOrEmpty(mode))
                mode = config.OutputMode.ToString().ToLower();

            mode = char.ToUpper(mode[0]) + mode[1..];
            var availableModes = new[] { "Text", "Bin", "Hex" };

            if (!availableModes.Contains(mode)) return;

            var message = config.MessageTemplate.Replace("{person}", Context.User.GetShortName());

            switch (Enum.Parse<GreetingOutputModes>(mode))
            {
                case GreetingOutputModes.Bin:
                    message = ConvertToBinOrHexa(message, 2);
                    break;
                case GreetingOutputModes.Hex:
                    message = ConvertToBinOrHexa(message, 16);
                    break;
                case GreetingOutputModes.Text:
                    message = config.MessageTemplate.Replace("{person}", Context.User.Mention);
                    break;
            }

            await ReplyAsync(message);
        }

        [Command("grillhi"), Alias("hi")]
        [Remarks("Možné základy soustav odpovědi jsou 2, 8, 10, nebo 16.")]
        public async Task GreetAsync(int @base)
        {
            var supportedBases = new[] { 2, 8, 10, 16 };

            if (!supportedBases.Contains(@base)) return;

            var config = await GetMethodConfigAsync<GreetingConfig>("", "grillhi");

            var message = config.MessageTemplate.Replace("{person}", Context.User.GetFullName());
            var converted = ConvertToBinOrHexa(message, @base);

            await ReplyAsync(converted);
        }

        private string ConvertToBinOrHexa(string message, int @base)
        {
            return string.Join(" ", message.Select(o => Convert.ToString(o, @base)));
        }

        [Command("kachna", true)]
        [Alias("duck")]
        [Summary("Zjištění aktuálního stavu kachny.")]
        public async Task GetDuckInfoAsync()
        {
            try
            {
                using var duckLoader = GetService<DuckDataLoader>();
                var duckRenderer = duckLoader.Scope.ServiceProvider.GetService<DuckEmbedRenderer>();

                var config = await GetMethodConfigAsync<DuckConfig>("kachna", null);
                var duckData = await duckLoader.Service.GetDuckCurrentStateAsync(config);

                var embed = duckRenderer.RenderEmbed(duckData, Context.User, config);
                await ReplyAsync(embed: embed.Build());
            }
            catch (WebException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("emojize")]
        [Summary("Vypíše slovo pomocí emotikonů.")]
        [Remarks("Pokud bude zadán parametr ID zprávy, tak slovo bude vyjmenováno do reakcí. Pokud se bude vypisovat pomocí reakcí, tak slovo nesmí obsahovat duplicitní znaky (až na výjimky). Duplicitní znaky mohou být pouze `A`, `B` a `O`. A to maximálně 1x navíc.\n" +
            "Pokud nebude zadán kanál, ale bude zadána zpráva, tak se bude zpráva vyhledávat v kanálu, kde byl zavolán příkaz.")]
        public async Task EmojizeWord(string word, IChannel channel = null, ulong messageId = 0)
        {
            try
            {
                if (channel == null) channel = Context.Channel;
                var emojized = EmojiHelper.ConvertStringToEmoji(word, messageId == 0);

                if (messageId != 0)
                {
                    using var messageCache = GetService<IMessageCache>();
                    var msg = await messageCache.Service.GetAsync(channel.Id, messageId);

                    if (msg == null)
                        return;

                    foreach (var emoji in emojized)
                    {
                        await msg.AddReactionAsync(emoji);
                    }
                }
                else
                {
                    if (channel is ISocketMessageChannel chnl)
                        await chnl.SendMessageAsync(string.Join(" ", emojized.Select(o => o.ToString())));
                }
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        #region Common

        private string CreateCachePath(string filename) => Path.Combine("ImageCache", filename);

        #endregion
    }
}
