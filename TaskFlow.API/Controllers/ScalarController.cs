using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.API.Controllers
{
    public class ScalarController : Controller
    {
        [HttpGet("/scalar")]
        public IActionResult Index()
        {
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "scalar.html"), "text/html");
        }
    }
}
