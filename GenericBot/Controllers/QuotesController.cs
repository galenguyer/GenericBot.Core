using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericBot.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GenericBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotesController : ControllerBase
    {
        private readonly ILogger<QuotesController> _logger;

        public QuotesController(ILogger<QuotesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{guildid}")]
        public IEnumerable<Quote> Get(ulong guildid)
        {
            return Core.GetAllQuotes(id);
        }
    }
}
