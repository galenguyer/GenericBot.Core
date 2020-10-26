using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenericBot.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // GET: api/v1/user/UserId
        [Authorize]
        [HttpGet("/api/v1/user")]
        public IActionResult Get()
        {
            ulong userId = ulong.Parse(User.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value);
            var user = Core.DiscordClient.GetUser(userId);

            var userData = new { 
                username = user.Username, 
                discriminator = user.Discriminator 
            };

            return new JsonResult(userData);
        }
    }
}
