// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Bokhantering.API.Models;
using Bokhantering.API.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;

namespace Bokhantering.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BokhanteringDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(BokhanteringDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST-metod för att registrera nya användare.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "Användarnamnet är redan upptaget." });
            }

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registrering lyckades!" });
        }

        // POST-metod för att logga in befintliga användare.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Ogiltigt användarnamn eller lösenord" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["Jwt:HemligNyckel"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("Jwt-hemlig nyckel är inte konfigurerad en AuthController.");
            }
            var key = Encoding.UTF8.GetBytes(secretKey);

            var currentTime = DateTime.UtcNow;
            var expirationTime = currentTime.AddHours(1);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                IssuedAt = currentTime,
                Expires = expirationTime,
                Issuer = _configuration["Jwt:ValidIssuer"],
                Audience = _configuration["Jwt:ValidAudience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }
    }
}