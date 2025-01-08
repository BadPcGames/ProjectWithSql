using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.DbModels;
using WebApplication1.Models;
using System.Text;
using WebApplication1.Services;

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

        var postsToShow = posts.Select(post => new PostViewModel
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
        return View(postsToShow);
    }

    // GET: One Post
    public async Task<IActionResult> ReadPost(int id)
    {

        var post = _context.Posts.FirstOrDefault(post => post.Id == id);
        PostViewModel postToShow = new PostViewModel()
        {
            Id = post.Id,
            Title = post.Title,
            BlogId = post.BlogId,
            Game = post.Game,
            CreateAt = post.CreatedAt,
            AuthorName = _context.Users.FirstOrDefault(
                    user => user.Id == _context.Blogs.FirstOrDefault(blog => blog.Id == post.BlogId).AuthorId)?.Name ?? "Unknown",
            BlogName = _context.Blogs.FirstOrDefault(blog => blog.Id == post.BlogId)?.Name ?? "Unknown",
            Contents = _context.Post_Contents.Where(postsContents => postsContents.PostId == post.Id).ToList()
        };

        return View(postToShow);
    }


    // GET: Posts/Create/5
    public IActionResult Create(int blogId)
    {
        var model = new Post
        {
            BlogId = blogId
        };
        return View(model);
    }

    // POST: Posts/Create/5
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
                var contentData= new byte[0];
                switch (content.ContentType)
                {
                    case "Text":
                        contentData = Encoding.UTF8.GetBytes(content.Content);
                        break;
                    default:
                        contentData = content.FormFile != null ? MyConvert.ConvertFileToByteArray(content.FormFile) : null;
                        break;
                }
               
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

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);
        if (post == null)
        {
            return NotFound();
        }

        int blogId = post.BlogId;

        var contentsToDelete = _context.Post_Contents.Where(m => m.PostId == post.Id);
        _context.Post_Contents.RemoveRange(contentsToDelete);
        var reactionsToDelete = _context.Reactions.Where(m => m.PostId == post.Id);
        _context.Reactions.RemoveRange(reactionsToDelete);
        var comentsToDelete = _context.Coments.Where(m => m.PostId == post.Id);
        _context.Coments.RemoveRange(comentsToDelete);
        _context.Posts.Remove(post);
        
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", new { id = blogId });
    }



    [HttpGet]
    public async Task<IActionResult> GetGames()
    {
        var games = await _context.Games.ToListAsync();
        return Json(games);
    }


}
