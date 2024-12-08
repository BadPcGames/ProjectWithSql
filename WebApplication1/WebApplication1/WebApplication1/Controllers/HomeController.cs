using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs.ToListAsync();
            var users = await _context.Users.ToListAsync();

            var blogsWithAuthors = blogs.Select(blog => new BlogViewModel
            {
                Id = blog.Id,
                Name = blog.Name,
                Description = blog.Description,
                Theme = blog.Theme,
                AuthorName = users.FirstOrDefault(user => user.Id == blog.AuthorId)?.Name ?? "Unknown"
            }).ToList();

            return View(blogsWithAuthors);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
