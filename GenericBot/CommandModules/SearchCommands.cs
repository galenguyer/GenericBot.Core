using GenericBot.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Wolfram.Alpha;
using Wolfram.Alpha.Models;
using UnitConversionLib;
using Unit = UnitConversionLib.Unit;

namespace GenericBot.CommandModules
{
    class SearchCommands
    {
        public List<Command> GetSearchCommands()
        {
            List<Command> searchCommands = new List<Command>();

            Command convert = new Command("convert");
            convert.Description = "Attempted smart unit conversion. Format: [value][unit] to [unit]";
            convert.ToExecute += async (client, msg, parameters) => 
            {
                try
                {
                    Measurable original;
                    if (parameters[1] == "to" || parameters[1] == "in")
                        original = Measurable.Parse(parameters[0]);
                    else
                        original = Measurable.Parse($"{parameters[0]} {parameters[1]}");

                    await msg.ReplyAsync(original.ConvertTo(parameters.Last()));
                }
                catch(Exception ex)
                {
                    await msg.ReplyAsync($"An error occured: {ex.Message}");
                    throw ex;
                }
            };
            searchCommands.Add(convert);

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
