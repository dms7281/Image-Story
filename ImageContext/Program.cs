using System.Web;
using ImageContext.Components;
using ImageContext.Components.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<GoogleServices>();
builder.Services.AddSingleton<OpenAIService>();

builder.Services.AddHttpClient("GoogleGeocoding", httpClient =>
{
    httpClient.BaseAddress = new Uri($"https://maps.googleapis.com/maps/api/geocode/");
});

builder.Services.AddHttpClient("GoogleSearch", httpClient =>
{
    httpClient.BaseAddress = new Uri($"https://www.googleapis.com/customsearch/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();