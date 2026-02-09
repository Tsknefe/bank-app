using BankApi.Application.Auth.Dtos;
using BankApi.Application.Auth.Security;
using BankApi.Domain.Entities;
using BankApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankApi.Api.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BankaDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(BankaDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var exist = await _context.Users.AnyAsync(x => x.Username == request.Username);
            if (exist) return BadRequest("Username already exist");

            var (hash, salt) = PasswordHasher.HashPassword(request.Password);
            var user = new User
            {
                Username = request.Username,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username && x.IsActive);
            if (user is null) return Unauthorized("Invalid credentials");

            var ok = PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!ok) return Unauthorized("Invalid credentials");

            var token = CreateToken(user);
            return Ok(new AuthResponse(token));
        }
        private string CreateToken(User user)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),

            };
            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["Expiresminutes"]!)),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);

            return Ok(
                new
                {
                    userId,
                    userName,
                    isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                });
        }
    }
}
