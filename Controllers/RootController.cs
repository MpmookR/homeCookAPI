using Microsoft.AspNetCore.Mvc;

namespace homeCookAPI.Controllers
{
    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("HomeCookAPI is running.");
        }
    }
}
