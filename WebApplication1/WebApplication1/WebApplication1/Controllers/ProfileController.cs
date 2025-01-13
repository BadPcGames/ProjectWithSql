using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;
using System.Text;
using WebApplication1.DbModels;
using WebApplication1.Models;
using WebApplication1.Services;
using MyConvert = WebApplication1.Services.MyConvert;


namespace WebApplication1.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        public ProfileController(AppDbContext context, IConfiguration config)
        {
            _context = context;
        }


        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = _context.Users.First(user => user.Id == int.Parse(HttpContext.User.FindFirst(ClaimTypes.System).Value));
            
            ViewBag.User = new UserViewModel()
            {
                Id = user.Id,
                Name = user.Name,
                Email= user.Email,
                Avatar = user.Avatar
            };
            var yourBlogs = _context.Blogs.Where(blog => blog.AuthorId ==
                int.Parse(HttpContext.User.FindFirst(ClaimTypes.System).Value)).ToList();
            return View(yourBlogs);
        }


    }
}

