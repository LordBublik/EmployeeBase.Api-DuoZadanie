using EmployeeBase.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace EmployeeBase.Client.Managers
{
    public class AuthManager
    {
        private readonly HttpClient _http;
        private readonly NavigationManager _nav;
        private readonly IJSRuntime _js;

        public AuthManager(HttpClient http, NavigationManager nav, IJSRuntime js)
        {
            _http = http;
            _nav = nav;
            _js = js;
        }

        public async Task<(bool Success, string ErrorMessage)> LoginAsync(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
                return (false, "Введите логин");
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Введите пароль");

            var request = new LoginRequestDto { Login = login, Password = password };

            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/login", request);

                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserInfoDto>();

                    await _js.InvokeVoidAsync("localStorage.setItem", "currentUser", JsonSerializer.Serialize(user));
                    await _js.InvokeVoidAsync("localStorage.setItem", "currentLogin", login);

                    _nav.NavigateTo("/employees");
                    return (true, "");
                }

                return (false, "Неверный логин или пароль");
            }
            catch (HttpRequestException)
            {
                return (false, "Не удалось подключиться к серверу API. Убедитесь, что он запущен.");
            }
            catch (Exception ex)
            {
                return (false, $"Произошла ошибка: {ex.Message}");
            }
        }

        public async Task<(bool Success, string ErrorMessage)> RegisterAsync(RegisterRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.NameToShow))
                return (false, "Введите отображаемое имя");
            if (string.IsNullOrWhiteSpace(request.Login))
                return (false, "Введите логин");
            if (string.IsNullOrWhiteSpace(request.Password))
                return (false, "Введите пароль");
            if (request.Password != request.ConfirmPassword)
                return (false, "Пароли не совпадают");

            try
            {
                var response = await _http.PostAsJsonAsync("api/auth/register", request);

                if (response.IsSuccessStatusCode)
                    return (true, "Регистрация успешна!");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error.Contains("уже существует")
                    ? "Такой логин уже занят"
                    : $"Ошибка регистрации: {response.StatusCode}");
            }
            catch (HttpRequestException)
            {
                return (false, "Не удалось подключиться к серверу API. Убедитесь, что он запущен.");
            }
            catch (Exception ex)
            {
                return (false, $"Произошла ошибка: {ex.Message}");
            }
        }
    }
}