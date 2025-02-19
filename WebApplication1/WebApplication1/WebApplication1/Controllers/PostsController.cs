using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.DbModels;
using WebApplication1.Models;
using System.Text;
using WebApplication1.Services;
using System.Security.Claims;
using Microsoft.Extensions.Hosting;

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

        if (HttpContext.User.FindFirst(ClaimTypes.System)?.Value != null)
        {
            ViewBag.CanChange= int.Parse(HttpContext.User.FindFirst(ClaimTypes.System)?.Value) == _context.Blogs.First(blog => blog.Id == id).AuthorId;
        }
        else
        {
            ViewBag.CanChange=false;
        }

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
    public async Task<IActionResult> Create(int blogId, [Bind("Title,Game")] Post post, List<PostContentViewModel> contents)
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
                await _context.SaveChangesAsync();
            } 
        }

        return RedirectToAction("Index", new { id = blogId });
    }

    // POST: Posts/Dlete/5
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

        var contentsToDelete = _context.Post_Contents.Where(m => m.PostId == id);
        _context.Post_Contents.RemoveRange(contentsToDelete);
        var reactionsToDelete = _context.Reactions.Where(m => m.PostId == id);
        _context.Reactions.RemoveRange(reactionsToDelete);
        var comentsToDelete = _context.Coments.Where(m => m.PostId == id);
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

    [HttpGet]
    public async Task<IActionResult> GetLikes(int postId)
    {
        int likeCount = await _context.Reactions
                                      .Where(like => like.PostId == postId && like.Value > 0)
                                      .CountAsync();
        return Ok(likeCount);
    }


    [HttpGet]
    public async Task<IActionResult> GetDisLikes(int postId)
    {
        int likeCount = await _context.Reactions
                                      .Where(like => like.PostId == postId && like.Value < 0)
                                      .CountAsync();
        return Ok(likeCount);
    }


    [HttpPost]
    public async Task<IActionResult> MakeReactions(int value, int postId)
    {
        if (HttpContext.User == null)
        {
            return Unauthorized(); 
        }

        int clientId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.System)?.Value);

        Reactions? existingReaction = _context.Reactions
                                              .FirstOrDefault(reaction => reaction.AuthorId == clientId && reaction.PostId == postId);

        if (existingReaction == null)
        {
            Reactions reactions = new Reactions()
            {
                Value = value,
                AuthorId = clientId,
                PostId = postId
            };

            _context.Add(reactions);
            await _context.SaveChangesAsync();
        }
        else
        {
            if (existingReaction.Value != value)
            {
                existingReaction.Value = value;
                _context.Update(existingReaction);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Reactions.Remove(existingReaction);
                await _context.SaveChangesAsync();
            }
        }

        return Ok(); 
    }

    [HttpGet]
    public async Task<IActionResult> GetComents(int postId)
    {
        var coments = await _context.Coments.Where(coment=>coment.PostId==postId).ToListAsync();
        var users = await _context.Users.ToListAsync();
        List<ComentViewModel> comentsToShow = coments.Select(coment => new ComentViewModel
        {
           Id = coment.Id,
           Text= coment.Text,
           PostId= coment.PostId,
           CanChange=coment.CreateAt.AddMinutes(15)>DateTime.Now,
           AuthorId= coment.AuthorId,
           AuthorName=users.FirstOrDefault(user=>user.Id==coment.AuthorId).Name,
           AuthorAvatar = users.FirstOrDefault(user => user.Id == coment.AuthorId).Avatar
        }).ToList();
        return Json(comentsToShow);
    }

    [HttpPost]
    public async Task<IActionResult> MakeComent(string text,int postId)
    {
        Console.WriteLine("sdfasdasda");
        if (HttpContext.User == null)
        {
            return Unauthorized();
        }

        int clientId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.System)?.Value);
        Coments newComent = new Coments()
        {
            AuthorId = clientId,
            Text= text,
            PostId= postId,
            CreateAt = DateTime.Now
        };

       _context.Coments.Add(newComent);
        await _context.SaveChangesAsync();

        return Ok();
    }

}
