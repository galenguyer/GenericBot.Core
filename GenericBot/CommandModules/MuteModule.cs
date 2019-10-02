using Discord;
using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericBot.CommandModules
{
    class MuteModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();

            Command mute = new Command("mute");
            mute.RequiredPermission = Command.PermissionLevels.Moderator;
            mute.Usage = "mute <user>";
            mute.Description = "Mute a user";
            mute.ToExecute += async (context) =>
            {
                if (context.Parameters.IsEmpty())
                {
                    await context.Message.ReplyAsync($"You need to specify a user!");
                    return;
                }
                var gc = Core.GetGuildConfig(context.Guild.Id);
                if (!context.Guild.Roles.Any(r => r.Id == gc.MutedRoleId))
                {
                    await context.Message.ReplyAsync("The Muted Role Id is configured incorrectly. Please talk to your server admin");
                    return;
                }
                var mutedRole = context.Guild.Roles.First(r => r.Id == gc.MutedRoleId);
                List<IUser> mutedUsers = new List<IUser>();
                foreach (var user in context.Message.GetMentionedUsers().Select(u => u.Id))
                {
                    try
                    {
                        await (context.Guild.GetUser(user)).AddRolesAsync(new List<IRole> { mutedRole });
                        mutedUsers.Add(context.Guild.GetUser(user));
                    }
                    catch
                    {
                    }
                }

                string res;

                if (mutedUsers.Count > 0)
                {
                    res = "Succesfully muted " + mutedUsers.Select(u => u.Mention).ToList().SumAnd();
                }
                else
                {
                    res = "Could not find that user";
                }

                await context.Message.ReplyAsync(res);
            };
            commands.Add(mute);


            return commands;
        }
    }
}
