using Microsoft.AspNetCore.Mvc;
using Portfolio_tracker.Data;
using Portfolio_tracker.DTOs;
using Portfolio_tracker.Models;

namespace Portfolio_tracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequestDto request)
        {
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = request.Password,
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
    }
}
