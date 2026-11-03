# Handoff: Infisical .NET SDK (Fork dreammaster38/infisical-dotnet-sdk)

Stand: 2026-11-03. Alle Arbeit ist auf `main` gemergt (zuletzt PR #7). Keine offenen PRs, keine uncommitteten Änderungen,
kein Tag, kein NuGet-Paket. Das ist so gewollt ("kein Tagging, keine NuGet-Pakete").

## Was existiert
- Typed Clients für den größten Teil der dokumentierten Infisical-API, erreichbar über `InfisicalClient` (Liste in `README.md`).
- Version 4.0.0 (`Infisical.Sdk.csproj`, `CHANGELOG.md`). Breaking Change: neue Modelltypen im globalen Namespace.
- XML-Doku, Source Link, eingebettete Symbole; CI-Builds deterministisch.
- Tests: `src/Infisical.Sdk.UnitTests` (108 Unit-Tests gegen Fake-Server, 11 Live-Tests mit `INFISICAL_LIVE_*`).
  Workflow `.github/workflows/tests.yml`; Release-Workflow `ci.yml` läuft nur bei Tags `*.*.*`.
- `test-instance/` (Skript für eine Wegwerf-Instanz), `docs/authentication-and-errors.md`, Beispiel `src/Infisical.Sdk.Test`.

## Offene Punkte

### Nicht verifiziert
1. **Docker-Weg von `test-instance/run-live-tests.sh`** (Docker Compose) wurde nie ausgeführt, in der Cloud-Session gibt es keinen Docker-Daemon. Nur der Weg ohne Docker ist getestet. Aufgabe: lokal mit Docker ausführen, Fehler beheben.
2. **Enterprise-Funktionen** (Gruppen, eigene Rollen, Project Templates, Gateway-Pools, Secret Rotations, Secret Scanning, SCIM-Tokens): nur gegen Backend-Quellcode geprüft, nicht gegen eine lizenzierte Instanz. Aufgabe: Instanz mit Lizenz, Live-Tests ergänzen.
3. **Echte Zielsysteme** für Secret Syncs, App Connections, Rotations (AWS, Postgres, ...) live nicht ausgeführt.
4. **NuGet-Paketinhalt:** ob die XML-Doku im Paket in `lib/<tfm>` liegt, ist ungeprüft (es durfte kein Paket gebaut werden). Prüfung, sobald Paketbau erlaubt ist: `dotnet pack` in ein Wegwerf-Verzeichnis und Inhalt ansehen.

### Entscheidungen des Nutzers
5. **Veröffentlichung:** Tag 4.0.0 und NuGet-Push über `ci.yml`. Vorher bewusst abgelehnt.
6. **Upstream:** Ob und wie die Änderungen in kleineren PRs an `infisical/infisical-dotnet-sdk` gehen.
7. **Altes Retry-Verhalten:** `GetAsync` (alte Aufrufe, `RetryPolicy`) wiederholt auch bei 4xx, ein 404 dauert ca. 14 s. Die neuen Clients nutzen `TransientRetryPolicy` (nur Netzwerkfehler, 5xx, 408). Bewusst nicht angefasst.
8. **Globaler Namespace:** In einer späteren Hauptversion könnten die Modelltypen in einen Namespace wandern.

### Mögliche Zusatzarbeit
9. **Drift-Checks** (Antwortstrukturen gegen Backend) liefen als einmalige Skripte; daraus wiederverwendbare Tests machen.
10. **Code- und Security-Review** der gesamten Änderung (empfohlener Einstieg).
11. **Instabile Endpunkte:** Secret Scanning und Inventory Views sind nicht Teil der öffentlichen API-Referenz.

## Wichtige Hinweise für die Weiterarbeit
- Entwicklungsbranch: `claude/festive-cori-uwm7z5`; nach einem Merge neu von `origin/main` aufsetzen (Branchname bleibt).
- PRs nur auf ausdrückliche Anfrage erstellen; der Nutzer sagt jeweils "Ja, erstelle den PR (und beobachte ihn)".
- Einige Endpunkte akzeptieren nur User-Login (Organisationen listen/lesen, SCIM-Tokens, Inventory-View anlegen), Machine-Identity-Tokens bekommen 403. Cert-Manager-Aufrufe per ID brauchen `SetCertManagerProject(projectId)`.
- Enum-Werte nur über die Konstanten-Klassen (`SecretSyncDestinations.*` usw.) setzen; Werte aus der Doku waren mehrfach falsch.
- Unit-Tests: `dotnet test src/Infisical.Sdk.UnitTests --filter "Category!=Live"`.
