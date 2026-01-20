using Microsoft.AspNetCore.Mvc;

namespace flea_WebProj.Controllers;

public class ErrorController : Controller
{
    [Route("Error/404")]
    public IActionResult NotFound404()
    {
        return View("NotFound");
    }
}