using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio_tracker.Data;
using Portfolio_tracker.DTOs;
using Portfolio_tracker.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;



namespace Portfolio_tracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly IConfiguration _configuration;
        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequestDto request)
        {
            if(_context.User.Any(x => x.Email == request.Email))
            {
                return BadRequest("Email already exits");
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreateAt = DateTime.Now
            };

            _context.User.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                Message = "Registration request is working",
                Data = request
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var user = await _context.User.FirstOrDefaultAsync(x => x.Email == request.Email);

            if(user == null)
            {
                return Unauthorized();
            }

            bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isValid)
            {
                return Unauthorized();
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Token = token
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["jwt:key"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["jwt:Issuer"],
                audience: _configuration["jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
                );

            var tokenHandler = new JwtSecurityTokenHandler();

            return tokenHandler.
                WriteToken(token);
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Authenticated");
        }
    }
}
