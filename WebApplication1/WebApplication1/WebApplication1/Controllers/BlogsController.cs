using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using System.Diagnostics;
using WebApplication1.DbModels;
using Microsoft.AspNetCore.Authorization;

public class BlogsController : Controller
{
    private readonly AppDbContext _context;

    public BlogsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Blogs

    [Authorize]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Blogs.ToListAsync());
    }

    // GET: Blogs/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Blogs/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,Theme,AuthorId")] Blog blog)
    {
        if (ModelState.IsValid)
        {
            // TODO 1: поменять на другой автор айди
            blog.AuthorId = 1;
            _context.Add(blog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(blog);
    }

    // GET: Blogs/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Blogs == null)
        {
            return NotFound();
        }

        var blog = await _context.Blogs.FindAsync(id);
        if (blog == null)
        {
            return NotFound();
        }
        return View(blog);
    }

    // POST: Blogs/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Theme")] Blog blog)
    {
        if (id != blog.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingBlog = await _context.Blogs.FindAsync(id);
                if (existingBlog == null)
                {
                    return NotFound();
                }

                existingBlog.Name = blog.Name;
                existingBlog.Description = blog.Description;
                existingBlog.Theme = blog.Theme;

                _context.Update(existingBlog);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(blog.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(blog);
    }

    // GET: Blogs/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.Id == id);
        if (blog == null)
        {
            return NotFound();
        }

        var postsToDelete = await _context.Posts.Where(m => m.BlogId == id).ToListAsync();
        foreach (var post in postsToDelete)
        {
            var contentsToDelete = _context.Post_Contents.Where(m => m.PostId == post.Id);
            _context.Post_Contents.RemoveRange(contentsToDelete);
            _context.Posts.Remove(post);
        }

        _context.Blogs.Remove(blog);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    private bool BlogExists(int id)
    {
        return _context.Blogs.Any(e => e.Id == id);
    }

    [HttpGet]
    public async Task<IActionResult> GetThemes()
    {
        var themes = await _context.Themes.ToListAsync();
        return Json(themes);
    }
}
