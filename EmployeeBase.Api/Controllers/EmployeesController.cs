using EmployeeBase.Api.Data;
using EmployeeBase.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EmployeeBase.Api.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public EmployeesController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        private int GetCurrentUserRoleId()
        {
            if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", "").Trim();
                if (_cache.TryGetValue(token, out UserSession session) && session != null)
                {
                    return session.RoleId;
                }
            }
            return 3;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? roleId = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null)
        {
            var query = _context.Employees
                .Include(e => e.Role)
                .AsQueryable();

            // Фильтр по ФИО
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(e => e.FullName.ToLower().Contains(searchLower));
            }

            // Фильтр по роли (если roleId передан и не равен 0)
            if (roleId.HasValue && roleId.Value > 0)
            {
                query = query.Where(e => e.RoleId == roleId.Value);
            }

            // Сортировка списка: fullname, profession, department, role. По умолчанию — по Id.
            bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            IOrderedQueryable<Employee> ordered = sortBy?.ToLowerInvariant() switch
            {
                "fullname" when desc => query.OrderByDescending(e => e.FullName),
                "fullname" => query.OrderBy(e => e.FullName),
                "profession" when desc => query.OrderByDescending(e => e.Profession),
                "profession" => query.OrderBy(e => e.Profession),
                "department" when desc => query.OrderByDescending(e => e.DepartmentId),
                "department" => query.OrderBy(e => e.DepartmentId),
                "role" when desc => query.OrderByDescending(e => e.Role!.Name),
                "role" => query.OrderBy(e => e.Role!.Name),
                _ when desc => query.OrderByDescending(e => e.Id),
                _ => query.OrderBy(e => e.Id)
            };

            query = ordered.ThenBy(e => e.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Profession = e.Profession,
                    Phone = e.Phone,
                    Email = e.Email,
                    DepartmentId = e.DepartmentId,
                    RoleId = e.RoleId,
                    RoleName = e.Role != null ? e.Role.Name : null
                })
                .ToListAsync();

            return Ok(new PagedResultDto<EmployeeDto>
            {
                Items = items,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                TotalCount = totalCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeDto dto)
        {
            int roleId = GetCurrentUserRoleId();
            if (roleId != 1 && roleId != 2)
                return StatusCode(403, new { message = "Недостаточно прав" });

            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Profession) ||
                string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Phone))
            {
                return BadRequest(new { message = "Все текстовые поля обязательны для заполнения" });
            }

            if (!dto.Email.Contains("@") || !dto.Email.Contains("."))
                return BadRequest(new { message = "Некорректный формат email" });

            bool deptExists = await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId);
            if (!deptExists)
                return BadRequest(new { message = $"Отдел с ID {dto.DepartmentId} не найден" });

            var employee = new Employee
            {
                FullName = dto.FullName,
                Profession = dto.Profession,
                DepartmentId = dto.DepartmentId,
                Phone = dto.Phone,
                Email = dto.Email,
                RoleId = dto.RoleId ?? 3
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Сотрудник успешно добавлен" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EmployeeDto dto)
        {
            int roleId = GetCurrentUserRoleId();
            if (roleId != 1 && roleId != 2)
                return StatusCode(403, new { message = "Недостаточно прав" });

            if (id != dto.Id) return BadRequest(new { message = "Несоответствие ID" });

            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Profession) ||
                string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Phone))
            {
                return BadRequest(new { message = "Все текстовые поля обязательны для заполнения" });
            }

            if (!dto.Email.Contains("@") || !dto.Email.Contains("."))
                return BadRequest(new { message = "Некорректный формат email" });

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound(new { message = "Сотрудник не найден" });

            bool deptExists = await _context.Departments.AnyAsync(d => d.Id == dto.DepartmentId);
            if (!deptExists)
                return BadRequest(new { message = $"Отдел с ID {dto.DepartmentId} не найден" });

            employee.FullName = dto.FullName;
            employee.Profession = dto.Profession;
            employee.DepartmentId = dto.DepartmentId;
            employee.Phone = dto.Phone;
            employee.Email = dto.Email;

            if (roleId == 1)
            {
                employee.RoleId = dto.RoleId;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Данные успешно обновлены" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int roleId = GetCurrentUserRoleId();
            if (roleId != 1 && roleId != 2)
                return StatusCode(403, new { message = "Недостаточно прав" });

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound(new { message = "Сотрудник не найден" });

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Сотрудник успешно удален" });
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
            return Ok(roles);
        }
    }
}