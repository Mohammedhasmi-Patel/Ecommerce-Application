using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [Route("api")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("test-error")]
        public IActionResult TestError()
        {
            throw new Exception("Something went wrong!");
        }
    }
}
