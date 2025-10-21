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
- Optional: Verwendet die von euch schon erstellte Benutzerverwaltung
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

Diese Einstellungen erstellen eine Blazor Server App mit Sample Pages und einer Benutzerverwaltung.
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
## Schritt 2 - Tweets und Likes
Anlegen der notwendigen Klassen bzw. Erweiterung der bestehenden Klassen und Migration in die Datenbank.
Optional Erweiterung der ApplicationDbContext Klasse für eine Fluent-API-Konfiguration
### ApplicationUser
Erweiterung um die Properties: Location, ProfilePicture, BackgroundPicture
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
Anlegen des DbSet für Tweets
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
private readonly Tweet _model = new();
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
_model.ApplicationUserId = userId;
DbContext.Tweets.Add(_model);
await DbContext.SaveChangesAsync();
// Neuen Tweet bereitstellen
_modelTweet = new Tweet();
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
// Letzte 10 Tweets laden, sortiert nach Erstellungsdatum absteigend
// Inkludiere die zugehörigen Benutzer und die Likes
_modelLastTweets = await DbContext.Tweets
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
