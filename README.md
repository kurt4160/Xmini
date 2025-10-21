# Xmini
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
PM> Update-Database

Jetzt kann die Applikation gestartet werden. Folgende Pages werden automatisch erstellt:
- Home: Die Startseite der Applikation
- Counter: Ein Sample für die Verwendung eines Buttons zur Interaktion mit einem Seiteninhalt
- Weather: Die Abfrage eines Wetter Service und Darstellung der Daten
- Auth Required: Eine Page die nur von angemeldeten Usern aufgerufen werden kann
- Register: Page zum registrieren eines Users
- Login: Page zum Anmelden eines Users




