using EmployeeBase.Api.Data;
using EmployeeBase.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EmployeeBase.Api.Controllers
{
    [ApiController, Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public AuthController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Неверный логин или пароль" });

            var token = Guid.NewGuid().ToString();

            int roleId = user.RoleId ?? 3;

            _cache.Set(token, new UserSession { RoleId = roleId }, TimeSpan.FromHours(8));

            return Ok(new UserInfoDto
            {
                NameToShow = user.NameToShow,
                RoleId = roleId,
                Token = token
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Login == request.Login))
                return BadRequest(new { message = "Пользователь уже существует" });

            var user = new User
            {
                NameToShow = request.NameToShow,
                Login = request.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = 3
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Успешно" });
        }
    }

    public class UserSession
    {
        public int RoleId { get; set; }
    }
}