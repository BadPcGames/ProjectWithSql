using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebApplication1.DbModels;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register([Bind("Name,Email,Password")] RegisterModel model)
        {

            if (!_context.Users.Any(m => m.Email == model.Email))
            {
                User user=new User
                {
                    Name= model.Name,
                    Email=model.Email,
                    PasswordHash=ShifrService.HashPassword(model.Password),
                    Role="User"
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                Login(new LoginModel()
                {
                    Email = model.Email,
                    Password=model.Password
                }); 
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index");
            }
  
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email,Password")] LoginModel model)
        {

            if(!_context.Users.Any(m => m.Email == model.Email))
            {
                //пользывателя нет с такой почтой
                return RedirectToAction("Index");
            }

            //проверка пароля пользывателя
            User user= _context.Users.FirstAsync(m => m.Email == model.Email).Result;
            if (!ShifrService.DeHashPassword(user.PasswordHash, model.Password))
            {
                return RedirectToAction("Index");
            }

            await SingIn(user);

            return RedirectToAction("Index", "Home");

        }

        private async Task SingIn(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.System,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Name),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Role,user.Role),
                new Claim(ClaimTypes.Actor,Encoding.UTF8.GetString(user.Avatar))
            };
            var claimsIdentyti = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name, ClaimTypes.Role);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentyti);
            await HttpContext.SignInAsync(claimsPrincipal);
        }

        public async Task<IActionResult> SingOut()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public int? GetUserId()
        {
            if (HttpContext.User.FindFirst(ClaimTypes.System)?.Value!=null)
            {
                return int.Parse(HttpContext.User.FindFirst(ClaimTypes.System)?.Value);
            }
            return null; 
        }

    }
}

