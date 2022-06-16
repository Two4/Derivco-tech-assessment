using Microsoft.AspNetCore.Mvc;

namespace DerivcoRoulette.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    [Route("/error")]
    public IActionResult HandleError()
    {
        return StatusCode(500);
    }
}