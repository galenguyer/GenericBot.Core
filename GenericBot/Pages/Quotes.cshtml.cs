using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericBot.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GenericBot.Pages
{
    public class QuotesModel : PageModel
    {
        public List<Quote> Quotes { get; set; }

        [BindProperty(SupportsGet = true)]
        public ulong GuildId { get; set; }

        public bool GuildIdIsValid { get; set; }

        public void OnGet()
        {
            GuildIdIsValid = GuildId != 0 && Core.DiscordClient.Guilds.Any(g => g.Id == GuildId);
            if (this.GuildIdIsValid)
            {
                Quotes = Core.GetAllQuotes(GuildId);
                Quotes.Reverse(); 
            }
        }
    }
}