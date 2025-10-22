using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xmini.Components;
using Xmini.Components.Account;
using Xmini.Data;



// Die statische Methode `WebApplication.CreateBuilder` erstellt und initialisiert eine Instanz
// von `WebApplicationBuilder`. Diese Methode bereitet die notwendige Infrastruktur für eine
// ASP.NET Core Webanwendung vor, ohne den Webserver zu starten. Wichtige Aspekte:
//   
// -Rückgabewert:
//   -Ein `WebApplicationBuilder`, das Zugriff auf zentrale Subsysteme gewährt:
//     - `Configuration`  : IConfiguration(Liest Konfigurationsquellen).
//     - `Environment`    : IWebHostEnvironment(Informationen zur Laufzeitumgebung).
//     - `Services`       : IServiceCollection(zur Registrierung von Diensten für DI).
//     - `Logging`        : ILoggingBuilder(zur Konfiguration von Logging - Providern).
//     - `WebHost`        : IWebHostBuilder(für WebHost - spezifische Einstellungen).
//   
// - Konfiguration / Quellen:
//   - `CreateBuilder` fügt standardmäßig mehrere Konfigurationsquellen hinzu:
//     - `appsettings.json`
//     - `appsettings.{ Environment}.json` (z.B. appsettings.Development.json)
//     - Umgebungsvariablen
//     - Befehlszeilenargumente (`args`)
//     - In der Entwicklungsumgebung werden optional User Secrets geladen.
//   - Diese Kombination stellt sicher, dass Einstellungen aus mehreren Ebenen verfügbar sind
//     und zur Laufzeit ausgewertet werden können.
//   
// - Logging:
//   -Standard - Logging - Provider(z.B.Console, Debug) werden registriert und können über
//     `builder.Logging` weiter konfiguriert werden.
//   
// - Dependency Injection:
//   -Eine `IServiceCollection` wird initialisiert, in die Framework- und Anwendungsdienste
//     (z. B. Kestrel, Routing, MVC/Blazor-Basiskomponenten) registriert werden können.
//   
// - WebHost-Defaults:
//   -Es werden WebHost-spezifische Standarddienste und -konfigurationen vorbereitet
//     (z. B. Kestrel-Server, ContentRoot, statische Dateien, URL-Bindings),
//     die spätere Aufrufe wie `builder.WebHost.ConfigureKestrel(...)` oder
//     Umgebungs-spezifische Anpassungen erlauben.
//   
// - Verhalten:
//   - `CreateBuilder` konfiguriert nichts dauerhaft — der tatsächliche `WebApplication`
//     wird erst durch Aufruf von `builder.Build()` erzeugt. Erst nach `app.Run()`
//     wird der Webserver gestartet und die Anwendung empfängt HTTP-Anfragen.
//   
// - Fazit:
//   - `WebApplication.CreateBuilder(args)` ist der Einstiegspunkt für die Minimal-Hosting-API,
//     der eine vorkonfigurierte Umgebung bereitstellt (Konfiguration, Logging, DI, WebHost),
//     so dass Entwickler anschließend Dienste registrieren und die Anwendung zusammenbauen
//     können, bevor sie gestartet wird.
var builder = WebApplication.CreateBuilder(args);

// builder.Services
// Typ: IServiceCollection
// Zweck: Registrieren eigener und framework‑weiter Dienste (z. B. AddControllers, AddRazorPages, AddAuthentication).
// Hier fügst du alle DI‑Services hinzu, die deine App später per Konstruktor erhält.

// AddRazorComponents ist eine Erweiterungsmethode für die Service-Registrierung (IServiceCollection),
// die in neueren ASP.NET‑Core/Blazor‑Versionen die nötige Infrastruktur für serverseitige Razor Components / interaktive Komponenten registriert.
// Sie sorgt dafür, dass die Services für das Rendern, Aktivieren und Verwalten von Komponenten
// (z. B. Renderer, ComponentActivator, JS‑Interop, Serialisierung/Options) verfügbar sind, damit Komponenten serverseitig gerendert und interaktiv gemacht werden können.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 32 * 1024 * 1024; // 32 MB
    });

// --- Authentifizierungs‑ und Identitätsdienste für Blazor ---

// AddCascadingAuthenticationState registriert in der DI‑Konfiguration die Infrastruktur, damit der AuthenticationState als
// Cascading Parameter in der Blazor‑Komponenten‑Hierarchie verfügbar wird. Dadurch können Komponenten wie AuthorizeView oder
// Komponenten, die per [CascadingParameter] den AuthenticationState anfordern, die aktuelle Anmeldeinformation erhalten.
//
// Wirkung / Einsatz:
//
// - Verknüpft die Plattform‑Authentifizierung mit Blazor‑Komponenten (stellt AuthenticationStateProvider / die nötigen Hilfsdienste bereit).
// - Macht Authentifizierungsdaten automatisch als Cascading Value verfügbar, sobald die App die Komponente CascadingAuthenticationState nutzt (z. B. in App.razor).
builder.Services.AddCascadingAuthenticationState();

// Was AddScoped macht
//   
// - Registrierung eines Dienstes mit Scoped‑Lifetime: Eine Instanz pro DI‑Scope.
// - In ASP.NET Core bedeutet das typischerweise „eine Instanz pro HTTP‑Request“ (Scope = IRequestScope).
// - In Blazor Server: Scope ≈ pro Circuit/Verbindung (also pro Benutzerverbindung).
// - In Blazor WebAssembly: Es gibt nur einen Root‑Scope, daher verhält sich Scoped dort praktisch wie Singleton (Achtung!).
// - Scoped‑Instanzen werden beim Ende des Scopes automatisch disposed (falls IDisposable).
//
// Typische Signaturen / Verwendung
//   
// - services.AddScoped<IMyService, MyService>();
// - services.AddScoped(sp => new MyService(...));
// - services.AddScoped();
//
// Alternativen(Kurzüberblick)
//   
// - AddSingleton: eine einzige Instanz für die gesamte Lebensdauer des ServiceProviders / der App. Gut für zustandslose,
//   threadsichere Services oder shared caches. Singleton darf keine Scoped‑Abhängigkeiten direkt injizieren.
// - AddTransient: neue Instanz bei jeder Anforderung (jede Injection / Aufruf). Gut für leichte, zustandslose Objekte.
// - TryAddScoped / TryAddSingleton etc. (aus Microsoft.Extensions.DependencyInjection.Extensions): registrieren nur,
//   wenn noch kein Eintrag existiert.
// 
// Wann welches wählen?
//   
// - Scoped: wenn Objekt zustandsbehaftet pro Request/Benutzerverbindung sein soll (z. B. DbContext in Webapps, per‑Request Kontextdaten).
// - Singleton: globaler service, teurer Init, shared state, threadsicher.
// - Transient: kurzlebige Objekte, keine gemeinsame State, oder wenn jede Verwendung eigene Instanz braucht.
//
// Wichtige Hinweise / Fallen
//   
// - Singleton darf keine Scoped‑Services direkt injizieren (führt zu Capturing‑Problem / Laufzeitfehler).
//   Lösung: inject IServiceProvider oder IServiceScopeFactory und erzeuge Scope bei Bedarf.
// - In Blazor WebAssembly ist Scoped nicht Request‑basiert — dort unterscheidet es sich von Server.
// - Disposal: Transient werden nur automatisch disposed, wenn vom DI erzeugt und der Scope endet; Singletons erst beim App‑Shutdown.

// IdentityUserAccessor ist ein kleiner, meist projekt­spezifischer Dienst, der den Zugriff auf die aktuell angemeldete Identität abstrahiert (ClaimsPrincipal / IdentityUser).
// Er kapselt z. B. IHttpContextAccessor oder in Blazor AuthenticationStateProvider, macht das Abrufen von Benutzer‑ID, UserName oder kompletten Benutzerobjekt testbar und
// vermeidet verbreitete Direktabhängigkeiten vom HttpContext in Geschäftslogik.
builder.Services.AddScoped<IdentityUserAccessor>();
// IdentityRedirectManager ist ein Hilfsdienst, der Weiterleitungen im Kontext von Identitätsoperationen (z. B. Anmeldung, Abmeldung, Zugriff verweigert)
builder.Services.AddScoped<IdentityRedirectManager>();
// Identity‑Revalidating‑AuthenticationStateProvider (häufig als RevalidatingIdentityAuthenticationStateProvider implementiert) ist ein Dienst für Blazor Server,
// der den AuthenticationState periodisch oder bei bestimmten Ereignissen erneut prüft und bei Ungültigkeit automatisch abmeldet.
//   
// Zweck: Sicherstellen, dass die angemeldete Identität aktuell ist (z. B. SecurityStamp geändert, Konto gesperrt oder gelöscht) und
// gegebenenfalls die Sitzung/AuthenticationState invalidieren.
//
// Wie es funktioniert: Periodischer Timer(konfigurierbares Intervall) ruft eine Validierungslogik auf, die typischerweise UserManager verwendet
// (SecurityStamp, Lockout, Existenz etc.). Wenn die Validierung fehlschlägt, wird der Benutzer abgemeldet.
//
// Einsatzgebiet: Blazor Server(nicht sinnvoll für Blazor WebAssembly).
// Lifetime: Scoped(pro Request / Circuit).
// Vorteile: Automatischer Schutz gegen stale Sessions nach Passwortänderung, Admin‑Deaktivierung etc.; zentrale Revalidierungslogik.
// Konfiguration: Intervall und konkrete Validierungslogik sind anpassbar; Registrierung über DI als AuthenticationStateProvider.
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Was AddAuthentication macht
//   
// Registriert die zentralen Authentication‑Services in der DI‑Pipeline und liefert ein AuthenticationBuilder‑Objekt zum Weiterkonfigurieren
// von Authentifizierungs‑Handlern (z.B. Cookie, JWT, OpenID Connect).
//   
// Rückgabe: AuthenticationBuilder — damit kannst du AddJwtBearer(), AddCookie(), AddOpenIdConnect() etc. anhängen.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

// --- Ende Authentifizierungs‑ und Identitätsdienste für Blazor ---

// Configure DbContextFactory mit SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// DbContextFactory erlaubt das Erzeugen von DbContext‑Instanzen bei Bedarf (z. B. in Hintergrunddiensten, Komponenten, Services)
// ohne direkte Abhängigkeit von der Scoped‑Lebensdauer eines DbContext.
// Vorteil: Vermeidet Probleme mit Scoped‑Lifetimes in nicht‑HTTP‑Kontexten und verbessert die Testbarkeit.
// Achtung: Ist eine Änderung gegenüber dem Code welches das Template generiert (AddDbContext).
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity mit Entity Framework Stores und Standard Token Provideren
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
// NoOp Email Sender (für Entwicklung / Test)
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Build the WebApplication
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Registriert einen Entwicklungs‑Endpoint für Entity‑Framework‑Core‑Migrationen, der es erlaubt, Informationen zu ausstehenden
    // Migrationen anzuzeigen und(in der Regel) Migrationen zur Laufzeit anzuwenden.
    // Wird typischerweise zusammen mit AddDatabaseDeveloperPageExceptionFilter verwendet und ist für die Entwicklungsumgebung gedacht.
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

// Registriert die Static‑Files‑Middleware in der ASP.NET‑Core‑Pipeline und liefert Dateien aus dem wwwroot‑Ordner
// (CSS, JS, Bilder, fonts, client‑App‑Assets) direkt ohne weitere Controller/Pages‑Logik.
// Wichtig für Blazor: sowohl Blazor Server als auch Blazor WebAssembly benötigen statische Dateien (z. B. index.html, _framework‑Assets, CSS/JS).
app.UseStaticFiles();
// In der Praxis ist „UseAntiforgery“ meist eine Middleware‑/Pipeline‑Schicht, die ein Antiforgery‑Token für Clients bereitstellt (z. B. als Cookie) –
// die eigentliche Validierung erfolgt explizit über IAntiforgery.ValidateRequestAsync.
// Zweck: Schutz vor Cross‑Site‑Request‑Forgery (CSRF) bei cookie‑basierter Authentifizierung (nicht nötig bei reinen Bearer‑Token/APIs).
//
// Use‑Cases: Single‑Page‑Apps mit Cookie‑Auth, Form‑Submits in klassischen MVC‑Apps.
app.UseAntiforgery();

// - MapRazorComponents ist ein Endpoint‑Routing‑Helper für serverseitige Razor‑Components / Blazor Server.
// - Registriert die nötigen Endpunkte für interaktive Komponenten (u. a. den SignalR‑Hub) und sorgt für das Fallback‑Routing zur Host‑Seite („_Host“),
//   sodass clientseitiges Routing der Komponenten funktioniert.
//
// Einsatz: nur für Blazor Server / Razor Components (nicht für Blazor WebAssembly).
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Eine Routing‑Extension, die zusätzliche Identity‑UI‑Endpoints in die Endpoint‑Pipeline registriert
// (z. B. Account/Manage‑Seiten, Passwort‑Reset, externe Login‑Callbacks usw.).
// Wird verwendet, wenn das Identity‑UI‑Paket (Microsoft.AspNetCore.Identity.UI / AddDefaultIdentity bzw. AddIdentity) eingesetzt wird.
app.MapAdditionalIdentityEndpoints();

app.MapGet("/images/{id}", async (int id, IDbContextFactory<ApplicationDbContext> factory) =>
{
    ApplicationDbContext dbContext = await factory.CreateDbContextAsync();
    var tweet = await dbContext.Tweets.FindAsync(id);
    if (tweet == null || tweet.Image == null || tweet.ContentType == null)
    {
        return Results.NotFound();
    }
    return Results.File(tweet.Image, tweet.ContentType);
});

app.Run();
