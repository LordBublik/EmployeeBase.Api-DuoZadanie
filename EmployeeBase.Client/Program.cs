using EmployeeBase.Client.Components;
using EmployeeBase.Client.Managers;

var builder = WebApplication.CreateBuilder(args);

// Добавляем компоненты Razor с интерактивным серверным режимом
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient, который будет стучаться на наш API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5257")
});

// Регистрируем наши "умные" менеджеры
builder.Services.AddScoped<AuthManager>();
builder.Services.AddScoped<EmployeeManager>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();