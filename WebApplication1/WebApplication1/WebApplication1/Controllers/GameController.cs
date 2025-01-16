using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using System.Diagnostics;
using WebApplication1.DbModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Reflection.Metadata;

public class GameController : Controller
{
    private readonly AppDbContext _context;

    public GameController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        List<Game> games=_context.Games.ToList();
        return View(games);
    }
    [Authorize(Roles ="Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string GameName,string Description,IFormFile GameCharacter,string Color)
    {
        Console.WriteLine(GameCharacter);

        Game game = new Game()
        {
            GameName = GameName,
            Description = Description,
            GameCharacter=MyConvert.ConvertFileToByteArray(GameCharacter),
            Color = Color
        };
        _context.Add(game);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Blogs == null)
        {
            return NotFound();
        }
        var game = await _context.Games.FindAsync(id);
        if (game == null)
        {
            return NotFound();
        }
        return View(game);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,string GameName, string Description, IFormFile? GameCharacter, string Color)
    {
        var existingGame = await _context.Games.FindAsync(id);
        if (existingGame == null)
        {
            return NotFound();
        }
        existingGame.GameName = GameName;
        existingGame.Description = Description;
        if(GameCharacter!=null) existingGame.GameCharacter =  MyConvert.ConvertFileToByteArray(GameCharacter);
        existingGame.Color = Color;
        _context.Update(existingGame);
        await _context.SaveChangesAsync();
       
        return RedirectToAction("Index");

    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var game=await _context.Games.FirstOrDefaultAsync(m => m.Id == id);
        if (game == null)
        {
            return NotFound();
        }

        var postsToDelete = await _context.Posts.Where(m => m.Game == game.GameName).ToListAsync();
        foreach (var post in postsToDelete)
        {
            var contentsToDelete = _context.Post_Contents.Where(m => m.PostId == post.Id);
            _context.Post_Contents.RemoveRange(contentsToDelete);
            var reactionsToDelete = _context.Reactions.Where(m => m.PostId == post.Id);
            _context.Reactions.RemoveRange(reactionsToDelete);
            var comentsToDelete = _context.Coments.Where(m => m.PostId == post.Id);
            _context.Coments.RemoveRange(comentsToDelete);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        _context.Games.Remove(game);

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}
