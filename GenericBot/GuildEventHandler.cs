using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using GenericBot.Entities;
using Newtonsoft.Json;

namespace GenericBot
{
    public static class GuildEventHandler
    {
        public static async Task OnJoinedGuild(SocketGuild guild)
        {
            if (File.Exists($"files/guildConfigs/{guild.Id}.json"))
            {
                GenericBot.GuildConfigs.Add(guild.Id, JsonConvert.DeserializeObject<GuildConfig>(
                    File.ReadAllText($"files/guildConfigs/{guild.Id}.json")));

                await GenericBot.Logger.LogGenericMessage($"Re-Joined Guild {guild.Id}({guild.Name}) Owned By {guild.Owner.Id}({guild.Owner})");

                string pref = !string.IsNullOrEmpty(GenericBot.GuildConfigs[guild.Id].Prefix)
                    ? GenericBot.GuildConfigs[guild.Id].Prefix
                    : GenericBot.GlobalConfiguration.DefaultPrefix;
                await guild.Owner.GetOrCreateDMChannelAsync().Result.SendMessageAsync($"I'm back on `{guild.Name}`! " +
                                                                                      $"If you don't remember me, go ahead and do `{pref}config` or `{pref}help`.");
            }
            else
            {
                await GenericBot.Logger.LogGenericMessage($"Joined Guild {guild.Id}({guild.Name}) Owned By {guild.Owner.Id}({guild.Owner})");
                await GenericBot.Logger.LogGenericMessage($"Creating GuildConfig for {guild.Id}");
                string joinMsg =
                    $"Hey, awesome! Looks like someone (maybe even you) just invited me to your server, `{guild.Name}`! " +
                    $"If you wanna see everything I can do out of the box, do `{GenericBot.GlobalConfiguration.DefaultPrefix}help`. " +
                    $"To set me up, do `{GenericBot.GlobalConfiguration.DefaultPrefix}config`";
                await guild.Owner.GetOrCreateDMChannelAsync().Result.SendMessageAsync(joinMsg);
                new GuildConfig(guild.Id).Save();
            }
        }

        public static async Task OnLeftGuild(SocketGuild guild)
        {
            await GenericBot.Logger.LogGenericMessage($"Left {guild.Id}({guild.Name})");
        }

    }
}
