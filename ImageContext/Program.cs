using ImageContext.Components;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var GoogleGeocodingAPIKey = builder.Configuration["GeocodingService:GoogleGeocodingApiKey"];
Console.WriteLine(GoogleGeocodingAPIKey);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

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