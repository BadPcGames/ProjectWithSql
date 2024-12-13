using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using System.Diagnostics;
using WebApplication1.DbModels;
using WebApplication1.Models;
using System.Linq;
using System.Text;

public class PostsController : Controller
{
    private readonly AppDbContext _context;

    public PostsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Posts
    public async Task<IActionResult> Index(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid blog ID.");
        }
        var posts = await _context.Posts
                                  .Where(post => post.BlogId == id)
                                  .ToListAsync();
        ViewBag.BlogId = id;

        var postsForShow = posts.Select(post => new PostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            BlogId = post.BlogId,
            Game = post.Game,
            CreateAt=post.CreatedAt,
            AuthorName = _context.Users.FirstOrDefault(
                user => user.Id == _context.Blogs.FirstOrDefault(blog => blog.Id == post.BlogId).AuthorId)?.Name ?? "Unknown",
            BlogName=_context.Blogs.FirstOrDefault(blog=>blog.Id==post.BlogId)?.Name ?? "Unknown",
            Contents=_context.Post_Contents.Where(postsContents=>postsContents.PostId==post.Id).ToList()
        }).ToList();
        return View(postsForShow);
    }

    // GET: Blogs/Create/5
    public IActionResult Create(int blogId)
    {
        var model = new Post
        {
            BlogId = blogId
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int blogId, [Bind("Title,Game")] Post post, List<Post_ContentViewModel> contents)
    {
        post.BlogId = blogId;
        post.CreatedAt = DateTime.Now;
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        if (contents != null)
        {
            foreach (var content in contents)
            {
                var contentData = content.ContentType switch
                {
                    "Text" => Encoding.UTF8.GetBytes(content.Content),
                    _ => content.FormFile != null ? await GetFileBytes(content.FormFile) : null
                };

                _context.Post_Contents.Add(new Post_Content
                {
                    PostId = post.Id,
                    ContentType = content.ContentType,
                    Content = contentData,
                    Position = content.Position
                });
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", new { id = blogId });
    }

    [HttpGet]
    public async Task<IActionResult> GetGames()
    {
        var games = await _context.Games.ToListAsync();
        return Json(games);
    }
    private async Task<byte[]> GetFileBytes(IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

}
