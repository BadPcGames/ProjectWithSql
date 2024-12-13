using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using System.Diagnostics;
using WebApplication1.DbModels;
using WebApplication1.Models;
using System.Linq;

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
        return View(blogId);
    }

    // POST: Blogs/Create/5
    [HttpPost]
    public async Task<IActionResult> Create(int blogId, [Bind("Id,Title,CreatedAt,Game")] Post post)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.BlogId = blogId;
            return View(post);
        }

        post.CreatedAt = DateTime.Now;
        post.BlogId = blogId;
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index", new { id = blogId });
    }


    //var existingContents = await _context.Post_Contents
    //    .Where(pc => pc.PostId == model.Id)
    //    .ToListAsync();

    //_context.Post_Contents.RemoveRange(existingContents);

    //foreach (var content in model.Contents)
    //{
    //    content.PostId = model.Id;
    //    _context.Post_Contents.Add(content);
    //}

    //// POST: Blogs/Create
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Create([Bind("Id,Name,Description,Theme,AuthorId")] Blog blog)
    //{
    //    if (ModelState.IsValid)
    //    {
    //        // TODO: поменять на другой автор айди
    //        blog.AuthorId = 1;
    //        _context.Add(blog);
    //        await _context.SaveChangesAsync();
    //        return RedirectToAction(nameof(Index));
    //    }
    //    return View(blog);
    //}

    //// GET: Blogs/Edit/5
    //public async Task<IActionResult> Edit(int? id)
    //{
    //    if (id == null || _context.Blogs == null)
    //    {
    //        return NotFound();
    //    }

    //    var blog = await _context.Blogs.FindAsync(id);
    //    if (blog == null)
    //    {
    //        return NotFound();
    //    }
    //    return View(blog);
    //}

    //// POST: Blogs/Edit/5
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Theme")] Blog blog)
    //{
    //    if (id != blog.Id)
    //    {
    //        return NotFound();
    //    }

    //    if (ModelState.IsValid)
    //    {
    //        try
    //        {
    //            var existingBlog = await _context.Blogs.FindAsync(id);
    //            if (existingBlog == null)
    //            {
    //                return NotFound();
    //            }

    //            existingBlog.Name = blog.Name;
    //            existingBlog.Description = blog.Description;
    //            existingBlog.Theme = blog.Theme;

    //            _context.Update(existingBlog);
    //            await _context.SaveChangesAsync();
    //        }
    //        catch (DbUpdateConcurrencyException)
    //        {
    //            if (!BlogExists(blog.Id))
    //            {
    //                return NotFound();
    //            }
    //            else
    //            {
    //                throw;
    //            }
    //        }
    //        return RedirectToAction(nameof(Index));
    //    }
    //    return View(blog);
    //}

    //// GET: Blogs/Delete/5
    //public async Task<IActionResult> Delete(int? id)
    //{
    //    if (id == null || _context.Blogs == null)
    //    {
    //        return NotFound();
    //    }

    //    var blog = await _context.Blogs
    //        .FirstOrDefaultAsync(m => m.Id == id);
    //    if (blog == null)
    //    {
    //        return NotFound();
    //    }

    //    return View(blog);
    //}

    //// POST: Blogs/Delete/5
    //[HttpPost, ActionName("Delete")]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> DeleteConfirmed(int id)
    //{
    //    if (_context.Blogs == null)
    //    {
    //        return Problem("Entity set 'AppDbContext.Blogs'  is null.");
    //    }
    //    var blog = await _context.Blogs.FindAsync(id);
    //    if (blog != null)
    //    {
    //        _context.Blogs.Remove(blog);
    //    }

    //    await _context.SaveChangesAsync();
    //    return RedirectToAction(nameof(Index));
    //}

    //private bool BlogExists(int id)
    //{
    //    return _context.Blogs.Any(e => e.Id == id);
    //}

    [HttpGet]
    public async Task<IActionResult> GetGames()
    {
        var games = await _context.Games.ToListAsync();
        return Json(games);
    }
}
