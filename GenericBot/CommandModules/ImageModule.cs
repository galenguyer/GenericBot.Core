using GenericBot.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GenericBot.CommandModules
{
    class ImageModule : Module
    {
        public List<Command> Load()
        {
            List<Command> commands = new List<Command>();
            Command cat = new Command("cat");
            cat.Description = "Link a cat pic";
            cat.SendTyping = false;
            cat.ToExecute += async (context) =>
            {
                var catStruct = new { file = string.Empty };
                using (var webclient = new WebClient())
                {
                    var catResult = JsonConvert.DeserializeAnonymousType(webclient.DownloadString(new Uri("http://aws.random.cat/meow")), catStruct);
                    await context.Message.ReplyAsync(catResult.file);
                }

            };
            commands.Add(cat);

            Command dog = new Command("dog");
            dog.Description = "Link a dog pic";
            dog.SendTyping = false;
            dog.ToExecute += async (context) =>
            {
                string url = string.Empty;
                using (var webclient = new WebClient())
                {
                    url = webclient.DownloadString(new Uri("https://random.dog/woof")); 
                    while (url.EndsWith("mp4")) 
                    {
                        url = webclient.DownloadString(new Uri("https://random.dog/woof")); 
                    }
                }
                await context.Message.ReplyAsync("http://random.dog/" + url);
            };
            commands.Add(dog);

            return commands;
        }
    }
}
