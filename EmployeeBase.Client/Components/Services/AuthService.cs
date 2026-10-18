using System.Net.Http.Json;
using EmployeeBase.Shared.DTOs;

namespace EmployeeBase.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;

        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<(bool Success, string Message, UserInfoDto? User)> LoginAsync(LoginRequestDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", dto);

                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserInfoDto>();
                    return (true, "", user);
                }

                return (false, "Неверный логин или пароль", null);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка соединения: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/register", dto);

                if (response.IsSuccessStatusCode)
                {
                    await response.Content.ReadAsStringAsync();
                    return (true, "Регистрация успешна! Теперь войдите в систему.");
                }

                var errorText = await response.Content.ReadAsStringAsync();

                if (errorText.Contains("Пароли не совпадают", StringComparison.OrdinalIgnoreCase))
                    return (false, "Пароли не совпадают");

                if (errorText.Contains("уже существует", StringComparison.OrdinalIgnoreCase))
                    return (false, "Пользователь с таким логином уже существует");

                return (false, $"Ошибка регистрации: {errorText}");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка соединения: {ex.Message}");
            }
        }
    }
}