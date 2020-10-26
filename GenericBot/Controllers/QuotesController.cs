using Microsoft.AspNetCore.Mvc;

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
    }
}
