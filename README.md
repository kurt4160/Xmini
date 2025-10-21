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
Als Connected Service wird eine Connection zur SQL Server Express LocalDB angelegt (für die Benutzerverwaltung)
