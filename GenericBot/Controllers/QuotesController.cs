using Microsoft.AspNetCore.Mvc;
using GenericBot.Entities;
using System.Collections.Generic;
using System.Linq;

namespace GenericBot.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        // GET api/v1/guilds/[GuildId]/quotes
        // GET all quotes for a guild
        [HttpGet("/api/v1/guild/{guildId}/quotes")]
        public IActionResult GetAllQuotes(ulong guildId)
        {
            return new JsonResult(Core.GetAllQuotes(guildId));
        }


        // GET api/v1/guilds/[GuildId]/quotes/[QuoteId]
        // GET quote for a guild by quoteId
        [HttpGet("/api/v1/guild/{guildId}/quotes/{quoteId}")]
        public IActionResult GetQuoteById(ulong guildId, int quoteId)
        {
            List<Quote> quotes = Core.GetAllQuotes(guildId);
            if (!quotes.Any(q => q.Id == quoteId))
            {
                return new NotFoundResult();
            }
            return new JsonResult(quotes.Find(q => q.Id == quoteId));
        }

        // GET api/v1/guilds/[GuildId]/quotes/random
        // GET a random quote for a guild
        [HttpGet("/api/v1/guild/{guildId}/quotes/random")]
        public IActionResult GetRandomQuote(ulong guildId, int quoteId)
        {
            List<Quote> quotes = Core.GetAllQuotes(guildId);
            if (!quotes.Any())
            {
                return new NotFoundResult();
            }
            return new JsonResult(quotes.GetRandomItem());
        }
    }
}
