using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SchedulerJobCenter.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("Healthy");

    }
}
