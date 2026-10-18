using System.Net.Http.Json;
using EmployeeBase.Shared.DTOs;

namespace EmployeeBase.Client.Services
{
    public class EmployeeService
    {
        private readonly HttpClient _http;

        public EmployeeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<PagedResultDto<EmployeeDto>> GetEmployeesAsync(int page, int pageSize = 10)
        {
            var result = await _http.GetFromJsonAsync<PagedResultDto<EmployeeDto>>(
                $"api/employees?page={page}&pageSize={pageSize}");
            return result ?? new PagedResultDto<EmployeeDto>();
        }
    }
}