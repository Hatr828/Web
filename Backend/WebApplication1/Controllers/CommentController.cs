using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CommentController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.Keys.Contains("formModel"))
            {
                ViewData["formModel"] = JsonSerializer.Deserialize<CommentModel>(
                    HttpContext.Session.GetString("formModel")!
                );
                HttpContext.Session.Remove("formModel");
            }
            return View();
        }
        public IActionResult Comment([FromForm] CommentModel formModel)
        {
            HttpContext.Session.SetString(
                "formModel",
                JsonSerializer.Serialize(formModel)
            );
            return RedirectToAction("Index");
        }
    }
}
