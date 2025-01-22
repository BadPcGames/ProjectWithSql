using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1;

[Authorize(Roles = "Moder,Admin")]
public class ModerController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
