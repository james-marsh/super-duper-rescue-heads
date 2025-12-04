using Microsoft.AspNetCore.Components.Authorization;
using SuperDuperRescueHeads.Web.Components;
using SuperDuperRescueHeads.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Authentication
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());

// Add authentication with a custom scheme for Blazor
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "BlazorScheme";
})
.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, CustomAuthenticationHandler>("BlazorScheme", options => { });

builder.Services.AddAuthorizationCore();

// HTTP Client for API calls
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5262";

// Register AuthTokenHandler as scoped
builder.Services.AddScoped<AuthTokenHandler>();

// Register authentication service with HTTP client (AddHttpClient registers the service automatically)
builder.Services.AddHttpClient<IAuthenticationService, AuthenticationService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Add a named HttpClient for general API calls (with auth token)
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthTokenHandler>();

// Register collection service
builder.Services.AddScoped<ICollectionService, CollectionService>();

// Register item service
builder.Services.AddScoped<IItemService, ItemService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
