using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Wolfram.Alpha;
using Wolfram.Alpha.Models;

namespace GenericBot.CommandModules
{
    class SearchCommands
    {
        public List<Command> GetSearchCommands()
        {
            List<Command> searchCommands = new List<Command>();

            Command wolfram = new Command("wolfram");
            wolfram.Aliases = new List<string> { };
            wolfram.Description = "Search using WolframAlpha";
            wolfram.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync("Please include a query for WolframAlpha");
                    return;
                }

                WolframAlphaService service = new WolframAlphaService(GenericBot.GlobalConfiguration.WolframAppId);
                var results = service.Compute(new Wolfram.Alpha.Models.WolframAlphaRequest(parameters.reJoin()));
                if (results == null)
                {
                    await msg.ReplyAsync("Something seems to have gone wrong.");
                }
                else
                {
                    string res = "";
                    int i = 0;
                    while(i < results.Result.QueryResult.Pods.Count)
                    {
                        string tmp = "";
                        var pod = results.Result.QueryResult.Pods[i];
                        tmp += pod.Title;
                        if (pod.SubPods != null)
                        {
                            foreach (SubPod subPod in pod.SubPods)
                            {
                                tmp += "  " + subPod.Title;
                                tmp += "  " + subPod.Plaintext;
                            }
                        }
                        if (tmp.Length + res.Length > 2000) break;
                        else res += tmp;
                    }
                    await msg.ReplyAsync(res);
                }
            };
            searchCommands.Add(wolfram);

            return searchCommands;
        }
    }
}
