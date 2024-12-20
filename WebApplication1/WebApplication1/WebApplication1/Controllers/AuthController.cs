using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        public IActionResult Login([Bind("Email,Password")] LoginModel model)
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
            
            string token = JWTGenareteServices.GenerateJwtToken(user.Id.ToString(),user.Role,_config);
            HttpContext.Response.Cookies.Append("tasty-cookies", token);
            return RedirectToAction("Index", "Home");
        }


    }
}

