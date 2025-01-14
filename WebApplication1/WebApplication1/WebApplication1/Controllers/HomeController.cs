using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.DbModels;
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
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetBlogs()
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

            return Json(blogsWithAuthors);
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

        public async Task<IActionResult> GetPosts(string? filterGame = null, string? filterTheme = null)
        {
            var blogs = await _context.Blogs.ToListAsync();
            var users = await _context.Users.ToListAsync();
            var posts = await _context.Posts.ToListAsync();
            var postContent = await _context.Post_Contents.ToListAsync();

            List<PostViewModel> Posts = posts.Select(post => new PostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                CreateAt = post.CreatedAt,
                Game = post.Game,
                BlogId = post.BlogId,
                BlogName = blogs.FirstOrDefault(blog => blog.Id == post.BlogId).Name ?? "Unknow",
                AuthorName = users.FirstOrDefault(user => user.Id == blogs.First(blog => blog.Id == post.BlogId).AuthorId)?.Name ?? "Unknown",
                Contents = postContent.Where(postContents => postContents.PostId == post.Id).ToList()
            }).ToList();

            if (filterGame != null)
            {
                Posts = Posts.Where(post => post.Game == filterGame).ToList();
            }
            if (filterTheme != null)
            {
                Posts = Posts.Where(post => _context.Blogs.First(blog => blog.Id == post.BlogId).Theme == filterTheme).ToList();
            }

            return Json(Posts);
        }

    }
}
