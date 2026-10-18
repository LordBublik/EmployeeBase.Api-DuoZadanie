using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmployeeBase.Shared.DTOs;
using Microsoft.JSInterop;

namespace EmployeeBase.Client.Managers
{
    public class EmployeeManager
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public EmployeeManager(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        private async Task<string?> GetTokenAsync()
        {
            var userJson = await _js.InvokeAsync<string>("localStorage.getItem", "currentUser");
            if (string.IsNullOrEmpty(userJson)) return null;
            var user = System.Text.Json.JsonSerializer.Deserialize<UserInfoDto>(userJson);
            return user?.Token;
        }

        private async Task AddAuthHeaderAsync()
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<PagedResultDto<EmployeeDto>> GetPageAsync(int page, int pageSize = 10)
        {
            if (page < 1) page = 1;
            await AddAuthHeaderAsync();
            var result = await _http.GetFromJsonAsync<PagedResultDto<EmployeeDto>>($"api/employees?page={page}&pageSize={pageSize}");
            return result ?? new PagedResultDto<EmployeeDto>();
        }

        public async Task<List<RoleDto>> GetRolesAsync()
        {
            await AddAuthHeaderAsync();
            return await _http.GetFromJsonAsync<List<RoleDto>>("api/employees/roles") ?? new List<RoleDto>();
        }

        private string ValidateEmployee(EmployeeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName)) return "Введите ФИО";
            if (string.IsNullOrWhiteSpace(dto.Profession)) return "Введите должность";
            if (dto.DepartmentId <= 0) return "Введите корректный ID отдела";
            if (string.IsNullOrWhiteSpace(dto.Phone)) return "Введите телефон";
            if (string.IsNullOrWhiteSpace(dto.Email)) return "Введите email";
            if (!dto.Email.Contains("@") || !dto.Email.Contains(".")) return "Некорректный email";
            return "";
        }

        public async Task<(bool Success, string Message)> CreateAsync(EmployeeDto dto)
        {
            var error = ValidateEmployee(dto);
            if (!string.IsNullOrEmpty(error)) return (false, error);
            await AddAuthHeaderAsync();
            var response = await _http.PostAsJsonAsync("api/employees", dto);
            if (response.IsSuccessStatusCode) return (true, "Сотрудник добавлен");
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, errorText.Contains("message") ? errorText : $"Ошибка {response.StatusCode}");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(EmployeeDto dto)
        {
            var error = ValidateEmployee(dto);
            if (!string.IsNullOrEmpty(error)) return (false, error);
            await AddAuthHeaderAsync();
            var response = await _http.PutAsJsonAsync($"api/employees/{dto.Id}", dto);
            if (response.IsSuccessStatusCode) return (true, "Данные обновлены");
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, errorText.Contains("message") ? errorText : $"Ошибка {response.StatusCode}");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            await AddAuthHeaderAsync();
            var response = await _http.DeleteAsync($"api/employees/{id}");
            if (response.IsSuccessStatusCode) return (true, "Сотрудник удален");
            return (false, $"Ошибка {response.StatusCode}");
        }
    }

    public class RoleDto { public int Id { get; set; } public string Name { get; set; } = null!; }
}