using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAPI.Models;
using WebAPI;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // DTOs auxiliares internos
        public record RegisterDto(string Username, string Password);
        public record LoginDto(string Username, string Password);
        public record ChangePasswordDto(string Username, string OldPassword, string NewPassword);

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<User>>> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return BadRequest(ApiResponse<User>.Fail("Usuario ya existente."));
            }

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = HashPassword(dto.Password),
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<User>.Succeed(user));
        }

        // POST: api/Auth/login (Genera token con 3 horas de vigencia)
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<string>>> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !user.IsActive || user.PasswordHash != HashPassword(dto.Password))
            {
                return BadRequest(ApiResponse<string>.Fail("Credenciales invalidas o usuario inactivo"));
            }

            var token = GenerateJwtToken(user);
            return Ok(ApiResponse<string>.Succeed(token, "Inicio de sesion correcto. Token valido por 3 horas."));
        }

        // PUT: api/Auth/changePassword
        [HttpPut("changePassword")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(ChangePasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || user.PasswordHash != HashPassword(dto.OldPassword))
            {
                return BadRequest(ApiResponse<object>.Fail("Usuario no encontrado o clave incorrecta"));
            }

            user.PasswordHash = HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Succeed(null));
        }

        // PUT: api/Auth/deactivate/5
        [HttpPut("deactivate/{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.Fail("Usuario no encontrado."));
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Succeed(null));
        }

        // DELETE: api/Auth/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.Fail("Usuario no encontrado."));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Succeed(null));
        }

        // Metodos de apoyo
        #region Helpers
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        #endregion
    }
}