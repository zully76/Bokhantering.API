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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                return BadRequest(new { message = "Användarnamnet är redan upptaget." });
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Registrering lyckades!" });
        }

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

    

