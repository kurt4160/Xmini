# Xmini
## Aufgabe
Schreibt eine Applikation die sich wie X verhält.
Beachtet folgende Vorgaben:
- Auf der Startseite werden alle Tweets von den Leuten angezeigt denen du folgst
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

## Schritt 1
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
## Schritt 2
Anlegen der notwendigen Klassen bzw. Erweiterung der bestehenden Klassen und Migration in die Datenbank.
Optional Erweiterung der ApplicationDbContext Klasse für eine Fluent-API-Konfiguration
### ApplicationUser
Erweiterung um die Properties: Location, ProfilePicture und BackgroundPicture
```
Add-Migration AddUserProperties
Update-Database
```



