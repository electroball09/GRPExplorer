using Microsoft.AspNetCore.Mvc;

namespace GRiPE.Server.Controllers
{
    [Route("shader/")]
    [ApiController]
    public class ShadersController : ControllerBase
    {
        [HttpGet("{fileName}")]
        public async Task<string> GetAsync(string fileName)
        {
            Console.WriteLine($"Sending shader {fileName}");

            return await System.IO.File.ReadAllTextAsync(Path.Combine("shader/", fileName));
        }
    }
}
