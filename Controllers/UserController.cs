using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApp.DTOs;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly TodoappContext _context;

        public UserController(TodoappContext context)
        {
            _context = context;
        }


        // POST: api/user/signup
        [HttpPost("signup")]
        public async Task<ActionResult<string>> SignUp([FromBody] SignupDTO request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already exists");

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = hashedPassword
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created successfully" });
        }

        // POST: api/user/login
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return BadRequest("Invalid email or password");

            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes("TopSecretKey11223344556677889900");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, user.Email), new Claim("UserId", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = "MyApp",  // Match the Audience in appsettings.json
                Issuer = "MyApp"
            };


            // Create the token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Return the token to the client
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { token = tokenString, message = "Login successful", username = user.Username });
        }
    }
}
