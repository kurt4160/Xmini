# Xmini
## Aufgabe
Schreibt eine Applikation die sich wie X verhält.
Beachtet folgende Vorgaben:
- Auf der Startseite werden die letzten 10 Tweets angezeigt
- Auf der Startseite kann man einen neuen Tweet erstellen (optional kann man ein Bild hinzufügen)
- Auf der „Entdecken“ Seite werden die Tweets mit den meisten „Likes“ angezeigt. Zusätzlich gibt es die Möglichkeit User zu suchen
- Tweets kann man „liken“
- Anderen Usern kann man folgen
- Über eine Profilseite kann man sich das Profil von Usern ansehen
  - Name
  - Standort
  - Profilbild (optional)
  - Hintergrundbild für Profil (optional)
- Optional: User können sich private Nachrichten schicken
- Optional: Anzeige der Anzahl der „Likes“ und die Anzahl der Follower unter jedem Tweet

## Schritt 1 - Neues Projekt
Erstellen des Blazor Projekts. Dazu die Projektvorlage "Blazor Web App" verwenden.
Bei den zusätzlichen Information folgende Einstellungen wählen:
- Framework: .NET 8.0 oder eine höhere LTS Version
- Authentication type: Individual Accounts
- Interactive render mode: Server
- Interactivity location: Global
- Include sample pages: yes
- Enlist in .NET Aspire orchestration: no

Diese Einstellungen erstellen eine Blazor Web App mit Sample Pages und einer Benutzerverwaltung.
Als Connected Service wird eine Connection zur SQL Server Express LocalDB angelegt (für die Benutzerverwaltung). Es wird dazu das EF verwendet. Die Migration wurde aber noch nicht durchgeführt.
Die Migration über die Package Manager Console starten (Menü View -> Other Windows): 
```
Update-Database
```

Jetzt kann die Applikation gestartet werden. Folgende Pages werden automatisch erstellt:
- Home: Die Startseite der Applikation
- Counter: Ein Sample für die Verwendung eines Buttons zur Interaktion mit einem Seiteninhalt
- Weather: Die Abfrage eines Wetter Service und Darstellung der Daten
- Auth Required: Eine Page die nur von angemeldeten Usern aufgerufen werden kann
- Register: Page zum registrieren eines Users
- Login: Page zum Anmelden eines Users
Da der DbContext nicht aus unterschiedlichen Threads verwendet werden kann, wird stattdessen eine DbContextFactory erzeugt. Über diese kann dann jederzeit ein DbContext geholt werden.
```
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```
## Schritt 2 - Tweets und Likes
Anlegen der notwendigen Klassen bzw. Erweiterung der bestehenden Klassen und Migration in die Datenbank.
Optional Erweiterung der ApplicationDbContext Klasse für eine Fluent-API-Konfiguration
### ApplicationUser
Verwenden der Klasse ApplicationUser die von IdentityUser ableitet. In dieser werden die zusätzlichen Properties des Users definiert.
Erweiterung um die Properties: Location, ProfilePicture, BackgroundPicture

todo ContentType der Bilder
```
Add-Migration AddUserProperties
Update-Database
```
### Tweet
Neue Klasse die die Tweets der User speichert: Id, Text, ApplicationUserId, CreatedAt
Anlegen der Navigationproperties in Tweet und ApplicationUser
```
public ICollection<Tweet>? Tweets { get; set; }
...
public ApplicationUser? ApplicationUser { get; set; }
```
Erweiterung in ApplicationDbContext zur Definition der 1:n Beziehung
```
builder.Entity<Tweet>()
    .HasOne(t => t.ApplicationUser)
    .WithMany(u => u.Tweets)
    .HasForeignKey(t => t.ApplicationUserId);
```
Anlegen des DbSet für Tweets
```
public DbSet<Tweet> Tweets { get; set; } = default!;
```
```
Add-Migration AddTweets
Update-Database
```
### Like
Neue Klasse die die Likes der User speichert. Hat Foreign Keys zu Tweets und ApplicationUser: Id, ApplicationUserId, TweetId
Anlegen der Navigationproperties in Like, Tweet und ApplicationUser
```
public Tweet? Tweet { get; set; }
...
public ICollection<Like>? Likes { get; set; }
...
public ICollection<Like>? Likes { get; set; }
```
Erweiterung in ApplicationDbContext zur Definition der 1:n Beziehungen
```
builder.Entity<Like>()
    .HasOne(l => l.ApplicationUser)
    .WithMany(u => u.Likes)
    .HasForeignKey(l => l.ApplicationUserId);
builder.Entity<Like>()
    .HasOne(l => l.Tweet)
    .WithMany(t => t.Likes)
    .HasForeignKey(l => l.TweetId);
```
Anlegen des DbSet für Likes
```
public DbSet<Like> Likes { get; set; } = default!;
```
```
Add-Migration AddLikes
Update-Database
```
## Schritt 3 - Umbau Home Page
### Aufruf nur mit Authorisierung
Wird über ein Attribut gesteuert
```
@attribute [Authorize]
```
### Formular mit Eingabefeld für Tweet und einem Post Button
Hier wird twoway DataBinding verwendet
```
<EditForm Model="_model" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />
    <div class="mb-3">
        <InputTextArea id="text" class="form-control" rows="3" @bind-Value="_model.Text" placeholder="Was gibt's Neues?" />
        <ValidationMessage For="@(() => _model.Text)" />
    </div>
    <button type="submit" class="btn btn-primary">Posten</button>
</EditForm>
```
Das Model ist hier ein Tweet Objekt
```
private Tweet _modelTweet = new();
```
### Validierung der Eingaben
Die Kriterien für die Valdidierung werden über Attribute bei den Properties gesteuert
```
[Required(ErrorMessage = "Der Text des Tweets ist erforderlich.")]
[MaxLength(280, ErrorMessage = "Der Text des Tweets darf maximal 280 Zeichen lang sein.")]
public string? Text { get; set; }
```
### Speichern
Nach der Ermittlung der Id des angemeldeten Users wird der Tweet in der Datenbank gespeichert
```
ApplicationDbContext dbContext = await Factory.CreateDbContextAsync();
// Sicherstellen, dass der Benutzer authentifiziert ist
if (_userId != null)
{
    // Tweet dem Benutzer zuordnen und speichern
    _modelTweet.ApplicationUserId = _userId;
    dbContext.Tweets.Add(_modelTweet);
    await dbContext.SaveChangesAsync();
    // Daten neu laden
    await LoadTweets();
    // Neuen Tweet bereitstellen
    _modelTweet = new Tweet();
}
```
## Schritt 4 - Liste der Tweets auf Home Page
Auf der Home Page soll eine Liste der aktuellsten 10 Tweets angezeigt werden.
Variable für die Tweets
```
private List<Tweet>? _modelLastTweets;
```
Eine Methode um die letzten 10 Tweets zu laden
```
private async Task LoadTweets()
{
    ApplicationDbContext dbContext = await Factory.CreateDbContextAsync();
    // Letzte 10 Tweets laden, sortiert nach Erstellungsdatum absteigend
    // Inkludiere die zugehörigen Benutzer und die Likes
    _modelLastTweets = await dbContext.Tweets
        .OrderByDescending(t => t.CreatedAt)
        .Include(u => u.ApplicationUser)
        .Include(l => l.Likes)
        .Take(10)
        .ToListAsync();
    // UI aktualisieren
    StateHasChanged();
}
```
Die Tweets sollen beim Initialiseren der Page und nach dem Speichern geladen werden
```
protected override async Task OnInitializedAsync()
{
    await LoadTweets();
    await base.OnInitializedAsync();
}
```
Die Liste der Tweets wird mit Razor Syntax erstellt
```
@if (_modelLastTweets != null && _modelLastTweets.Count > 0)
{
  foreach (var tweet in _modelLastTweets)
  {
      @* mb-3: margin-bottom 3 *@
      <div class="mb-3">
          @* fw-bold: font-weight bold *@
          <div class="fw-bold">
              @* Username und Create Time *@
              @((tweet.ApplicationUser?.UserName) ?? tweet.ApplicationUserId ?? "Unknown")
              (@tweet.CreatedAt.ToLocalTime())
          </div>
          @* white-space: pre-wrap; sorgt dafür, dass Zeilenumbrüche im Text erhalten bleiben *@
          <div style="white-space: pre-wrap;">
              @((tweet.Text) ?? string.Empty)
          </div>
      </div>
      <hr />
  }
```
## Schritt 5 - Like Button hinzufügen
Unter jedem Tweet soll ein Button angezeigt werden. Neben dem Button die Anzahl der Likes für diesen Tweet.
Der Button soll den Tweet liken oder das Like entfernen wenn es bereist vom User geliked wurde.
Button in anderem Style und Text mit Like im Razor hinzufügen
```
<div>
    <button class="btn btn-secondary" @onclick="() => OnLikeTweet(tweet)">Like</button>
    @((tweet.Likes?.Count) ?? 0)
</div>
```
Methode OnLikeTweet hinzufügen
```
private async Task OnLikeTweet(Tweet tweet)
{
    ApplicationDbContext dbContext = await Factory.CreateDbContextAsync();
    var newLike = new Like
    {
        TweetId = tweet.Id,
        ApplicationUserId = _userId
    };
    // Prüfen, ob der Benutzer den Tweet bereits geliked hat
    var existingLike = await dbContext.Likes
        .FirstOrDefaultAsync(l => l.TweetId == tweet.Id && l.ApplicationUserId == _userId);
    if (existingLike != null)
    {
        // Like entfernen
        dbContext.Likes.Remove(existingLike);
    }
    else
    {
        // Neues Like hinzufügen
        dbContext.Likes.Add(newLike);
    }
    await dbContext.SaveChangesAsync();
    // Daten neu laden
    await LoadTweets();
}
```
## Schritt 6 - Users folgen
In der Titelzeile eines Tweets soll ein Button die Möglichkeit bieten dem User des Tweets zu folgen. Bei nochmaligem Drücken soll nicht mehr gefolgt werden.
Der Text des Buttons soll immer den passenden Text anzeigen ("Folgen" bzw. "Nicht folgen").
### Klasse für Followers
Neue Klasse Followers
```
public class Followers
{
    public int Id { get; set; }
    // Foreign key zur bestehenden Identity-Klasse
    // User der dem anderen User folgt
    public string? FollowerUserId { get; set; }
    // Foreign key zur bestehenden Identity-Klasse
    // User der gefolgt wird
    public string? FollowsUserId { get; set; }

    // Navigation properties
    public ApplicationUser? FollowerUser { get; set; }
    public ApplicationUser? FollowsUser { get; set; }
}
```
ApplicationDbContext anpassen
```
...
public DbSet<Followers> Followers { get; set; } = default!;
...
// Konfiguration der Beziehung zwischen ApplicationUser und Followers
builder.Entity<Followers>()
    .HasOne(f => f.FollowerUser)
    .WithMany(u => u.Followers)
    .HasForeignKey(f => f.FollowerUserId);
// Konfiguration der Beziehung zwischen ApplicationUser und Following
builder.Entity<Followers>()
    .HasOne(f => f.FollowsUser)
    .WithMany(u => u.Following)
    .HasForeignKey(f => f.FollowsUserId);
...
NavigationProperties in ApplicationUser ergänzen
```
// Navigation properties für die Beziehung zu Follower
public ICollection<Follower>? Followers { get; set; }
public ICollection<Follower>? Following { get; set; }
```
```
### Page anpassen
Die Liste der Tweets etwas schöner darstellen:
```
    // Alle geladenen Tweets anzeigen
    // Hierzu Razor-Syntax verwenden
    foreach (var tweet in _modelLastTweets)
    {
        // Context für die Datenbank erstellen
        ApplicationDbContext dbContext = Factory.CreateDbContext();

        <ul class="list-unstyled">
            @* mb-3: margin-bottom 3 *@
            <li class="mb-3">
                <div class="border rounded p-3">
                    @* -- Zeile 1: links Text, rechts Button *@
                    @* Content wird in einer flexbox dargestellt *@
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <div class="fw-bold">
                            @* Username und Create Time *@
                            @((tweet.ApplicationUser?.UserName) ?? tweet.ApplicationUserId ?? "Unknown")
                            (@tweet.CreatedAt.ToLocalTime())
                        </div>
                        @if (tweet.ApplicationUserId != _userId)
                        {
                            <button type="button" class="btn btn-sm btn-outline-primary" @onclick="() => OnFollowUser(tweet)">
                                @* Prüfen, ob der aktuelle Benutzer dem Tweet-Ersteller folgt und entsprechend den Button-Text anpassen *@
                                @if (dbContext.Followers.Any(f => f.FollowerUserId == _userId && f.FollowsUserId == tweet.ApplicationUserId))
                                {
                                    <span>Nicht folgen</span>
                                }
                                else
                                {
                                    <span>Folgen</span>
                                }
                            </button>
                        }
                    </div>
                    @* Zeile 2: mehrzeiliger Text *@
                    @* White-space pre-wrap sorgt dafür, dass Zeilenumbrüche im Text erhalten bleiben *@
                    <div class="mb-2" style="white-space: pre-wrap;">
                        <p class="mb-0">
                            @((tweet.Text) ?? string.Empty)
                        </p>
                    </div>
                    @* Zeile 3: Button (links) *@
                    <div>
                        <button type="button" class="btn btn-sm btn-outline-primary" @onclick="() => OnLikeTweet(tweet)">Like</button>
                        @((tweet.Likes?.Count) ?? 0)
                    </div>
                </div>
            </li>
        </ul>
    }
}
```
Methode OnFollowUser implementieren
```
private async Task OnFollowUser(Tweet tweet)
{
    ApplicationDbContext dbContext = await Factory.CreateDbContextAsync();
    // Prüfen, ob der aktuelle Benutzer dem Tweet-Ersteller bereits folgt
    var existingFollower = await dbContext.Followers.FirstOrDefaultAsync(f => f.FollowsUserId == tweet.ApplicationUserId);
    if (existingFollower != null)
    {
        // Folgen aufheben
        dbContext.Followers.Remove(existingFollower);
    }
    else
    {
        // Neuen Follower hinzufügen
        var newFollower = new Followers
        {
            FollowerUserId = _userId,
            FollowsUserId = tweet.ApplicationUserId
        };
        dbContext.Followers.Add(newFollower);
    }
    await dbContext.SaveChangesAsync();
    // Daten neu laden
    await LoadTweets();
}
```
## Schritt 7 - Bilder in Tweets
An einen Tweet soll ein Bild angefügt werden können.
### Erweiterung der Klasse Tweet
Das Bild wird als byte[] gespeichert. Zusätzlich wird der ContentType benötigt.
```
public byte[]? Image { get; set; }
public string? ContentType { get; set; }
```
### Button zum Hochladen des Bildes
Da sich <InputFile> nicht gut stylen lässt, verstecken wir dies in einem <label>
```
<label class="btn btn-sm btn-outline-primary">
    Medien
    <InputFile OnChange="OnInputFileChange" accept="image/*" style="display:none" />
</label>
```
In der Methode OnInputFileChange wird geprüft ob die die Dateigröße passt und es der ContentType "image" ist.
Die Bilder sollen vor dem Hochladen im Browser auf eine max. Größe skaliert werden:
```
var resized = await file.RequestImageFileAsync(file.ContentType, 800, 600);
```
Der FileStream wird dann über einen MemoryStream im byte[] gespeichert:
```
using var ms = new MemoryStream();
await resized.OpenReadStream(_maxFileSize).CopyToAsync(ms);
_modelTweet.Image = ms.ToArray();
```
Die Vorschau des Bildes wird als base64 direkt im <image> eingebettet:
```
<img src="@ImagePreview" alt="Vorschau" class="img-thumbnail" style="max-width:500px;" />
...
// Vorschau-URL für das Bild
private string? ImagePreview => _modelTweet.Image != null && _modelTweet.ContentType != null
    ? $"data:{_modelTweet.ContentType};base64,{Convert.ToBase64String(_modelTweet.Image)}"
    : null;
```
### Bilder in der Liste der Tweets anzeigen
Um die Page mit vielen eingebetteten base64 Bildern nicht unnötig zu vergrößeren, legen wir zum Lesen der Bilder eine Minimal API im Program.cs an
```
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
```
Über die Url /images und der Id des Tweets wird hier das Bild aus der Datenbank gelesen und zum Browser geschickt.
Eine Minimal API kann einfach und resourcenschonend Daten zur Verfügung stellen. Auch komplexe REST APIs lassen sich damit ohne MVC realisieren und sind ab .NET 8 zu bevorzugen.
Das Bild wird unter dem Text angezeigt:
```
@* Zeile 3: Bild (falls vorhanden) *@
@if (tweet.Image != null)
{
    <div class="mt-3">
        <img src="/images/@tweet.Id" alt="Bild" class="img-fluid" />
    </div>
}
```
## Schritt 8 - Eine eigene User Page
Der Username in der Liste der Tweets soll ein Link zur einer eigenen Page sein. Auf der Page werden öffentliche Informationen zum User und dessen Tweets angezeigt.
Neue Razor Page UserInfo.razor anlegen. Diese wird über eine Url aufgerufen in der die Id des Users mit übergeben wird.
```
@page "/userinfo/{id}"
...
[Parameter]
public string? Id { get; set; }
```
Ein Großteil des Codes und des Layouts kann aus der Home Page übernommen werden. Später werden wir das in einer Komponente wiederverwendbar abbilden.
LoadTweets wird entsprechend angepasst, sodass nur die Tweets des Users gelesen werden:
```
private async Task LoadTweets()
{
    ApplicationDbContext dbContext = await Factory.CreateDbContextAsync();
    // Letzte 10 Tweets des Users laden, sortiert nach Erstellungsdatum absteigend
    // Inkludiere die zugehörigen Benutzer und die Likes
    _modelLastTweets = await dbContext.Tweets
        .Include(u => u.ApplicationUser)
        .Include(l => l.Likes)
        .Where(t => t.ApplicationUserId == Id)
        .OrderByDescending(t => t.CreatedAt)
        .Take(10)
        .ToListAsync();
    // UI aktualisieren
    StateHasChanged();
}
```
In der Liste der Tweets wird der User als Link angezeigt:
```
 @if (tweet.ApplicationUserId != null)
 {
     <a href="/UserInfo/@tweet.ApplicationUserId" >@(tweet.ApplicationUser?.UserName)</a>
 }
 else
 {
     <span>Unbekannt</span>
 }
```
