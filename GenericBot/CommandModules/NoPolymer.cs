using System;
using System.Collections.Generic;
using System.Text;
using GenericBot.Entities;
using GenericBot;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace GenericBot.CommandModules
{
    public class NoPolymer
    {
        public List<Command> GetPolyCommands()
        {
            Command nopoly = new Command("nopoly");
            nopoly.Description = "Use to opt in or out of notifications about the nopoly for firefox extension";
            nopoly.Usage = "nopoly <enroll|unenroll>";
            nopoly.ToExecute += async (client, msg, parameters) =>
            {
                var manifest = new Manifest(1);
                if (parameters.Empty())
                {
                    await msg.ReplyAsync("Parameters are missing");
                    return;
                }
                else if (parameters[0].ToLower().Equals("enroll"))
                {
                    if (manifest.EnrolledUsers.Contains(msg.Author.Id))
                    {
                        try
                        {
                            await msg.Author.GetOrCreateDMChannelAsync().Result.SendFileAsync("files/youtube_nopolymer.xpi", text: $"Latest Changelog: {manifest.Changelog}");
                            await msg.ReplyAsync("You're already enrolled! I've sent you the latest update");
                        }
                        catch
                        {
                            await msg.ReplyAsync("You're already enrolled! I tried sending you the latest version but I wasn't able to");
                        }
                    }
                    else
                    {
                        try
                        {
                            string message = "Thank you for enrolling in updates to the NoPolymer for Youtube extension! It aims to provide a faster, lighter weight" +
                            "YouTube experience. You can read more about the issue here: <https://twitter.com/cpeterso/status/1021626510296285185>. If you want to see the " +
                            "source for this plugin, see https://gitlab.com/MasterChief-John-117/Firefox-Youtube-NoPolymer. \n\n **To Install:**" +
                            "\n1) Open `about:addons` " +
                            "\n2) Click the gear icon in the top right" +
                            "\n3) Select \"Install Add-on From File\"" +
                            "\n4) Select the downloaded xpi file";
                            await msg.Author.GetOrCreateDMChannelAsync().Result.SendFileAsync("files/youtube_nopolymer.xpi", text: message);
                            manifest.EnrolledUsers.Add(msg.Author.Id);
                            manifest.Save();
                            await msg.ReplyAsync("Thanks for enrolling! You've been sent install instructions along with a bit of background. You'll be sent updates automatically. " +
                                "If your friends want to use this extension as well, please get them to enroll with the bot so they can get updates as well");
                        }
                        catch
                        {
                            await msg.ReplyAsync("You're already enrolled! I tried sending you the latest version but I wasn't able to");
                        }
                    }
                }
                else if (parameters[0].ToLower().Equals("unenroll"))
                {
                    if (manifest.EnrolledUsers.Contains(msg.Author.Id))
                    {
                        manifest.EnrolledUsers.Remove(msg.Author.Id);
                        manifest.Save();
                        await msg.ReplyAsync("You've been unenrolled from updates.");
                    }
                    else
                    {
                        await msg.ReplyAsync("You were not enrolled");
                    }
                }
                else
                {
                    await msg.ReplyAsync("Unrecognized parameter");
                }
            };

            return new List<Command> { nopoly };
        }
    }

    public class Manifest
    {
        public string Changelog;
        public List<ulong> EnrolledUsers;

        public Manifest(int val)
        {
            if (File.Exists("files/nopoly_manifest.json"))
            {
                var tmp = JsonConvert.DeserializeObject<Manifest>("files/nopoly_manifest.json");
                this.Changelog = tmp.Changelog;
                this.EnrolledUsers = tmp.EnrolledUsers;
            }
            else
            {
                Changelog = "No changelog";
                EnrolledUsers = new List<ulong>();
            }
        }
        public Manifest()
        {

        }
        public void Save()
        {
            File.WriteAllText("files/nopoly_manifest.json", JsonConvert.SerializeObject(this, formatting: Formatting.Indented));
        }
    }
}
