using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xmini.Data;
using Xmini.data.Entities;
using Xmini.shared.Dto;
using Xmini.wasm.Components;
using Xmini.wasm.Components.Account;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Xmini.wasm.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapGet("api/followers/{id}", async (string id, IDbContextFactory<ApplicationDbContext> factory) =>
{
    ApplicationDbContext dbContext = factory.CreateDbContext();
    List<FollowerDto> followers = await dbContext.Followers
        .Select(f => new FollowerDto(f.Id, f.FollowerUserId, f.FollowsUserId))
        .Where(f => f.FollowerUserId == id)
        .ToListAsync();
    return Results.Ok(followers);
});
app.MapGet("api/tweets/last", async (IDbContextFactory<ApplicationDbContext> factory) =>
{
    ApplicationDbContext dbContext = factory.CreateDbContext();
    // Letzte 10 Tweets laden, sortiert nach Erstellungsdatum absteigend
    // Inkludiere die zugehörigen Benutzer und die Likes
    List<TweetDto> tweets = await dbContext.Tweets
        .OrderByDescending(t => t.CreatedAt)
        .Include(u => u.ApplicationUser)
        .Include(l => l.Likes)
        .Take(10)
        .Select(t => new TweetDto()  {Id = t.Id, Text = t.Text, CreatedAt = t.CreatedAt, UserId = t.ApplicationUserId, HasImage = (t.Image != null && t.ContentType != null)})
        .ToListAsync();
    return Results.Ok(tweets);
});
app.Run();
