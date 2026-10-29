# Opsætning af Synapse Backend fra GitHub

Denne guide viser den minimale opsætning, der skal til for at køre Synapse Backend lokalt efter projektet er hentet fra GitHub.

## Krav

Følgende skal være installeret:

- .NET 10 SDK
- MySQL Server
- Git
- dotnet-ef version 9.0.2

Installer EF CLI hvis nødvendigt:

```powershell id="7y4ybr"
dotnet tool install --global dotnet-ef --version 9.0.2
```

## Hent projektet

```powershell id="ddtb0j"
git clone <repository-url>
cd SynapseBackend
```

Projektet består blandt andet af:

```text id="yr1y7i"
Synapse
Synapse.Application
Synapse.Domain
Synapse.Infrastructure
Synapse.Tests
```

## Installer dependencies

Kør fra solution-mappen:

```powershell id="pw8q75"
dotnet restore
dotnet build
```

Build skal gennemføres uden fejl.

## Opsæt MySQL

MySQL Server skal køre lokalt.

Projektet bruger databasen:

```text id="u3y7i6"
synapse
```

Standard development setup:

```text id="5unqeg"
Server: localhost
Port: 3306
Database: synapse
User: root
```

## Første gangs opsætning

Gå ind i API-projektet:

```powershell id="2n9c48"
cd Synapse
```

Initialiser User Secrets:

```powershell id="m4z04j"
dotnet user-secrets init
```

Sæt JWT key:

```powershell id="k6j76g"
dotnet user-secrets set "Jwt:Key" "YOUR_KEY"
```

Sæt database connection string:

```powershell id="nwwae0"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
```

Opdater databasen:

```powershell id="i9f6cb"
dotnet ef database update
```

Start backend:

```powershell id="1c4tmw"
dotnet run
```

Hele opsætningen samlet:

```powershell id="om6w52"
cd Synapse
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "YOUR_KEY"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
dotnet ef database update
dotnet run
```

Eksempel på en lokal MySQL connection string:

```text id="wtid3w"
Server=localhost;Port=3306;Database=synapse;User=root;Password=DIT_PASSWORD;
```

## Kontroller User Secrets

Du kan kontrollere de lokale værdier med:

```powershell id="ju3aav"
dotnet user-secrets list
```

Der skal som minimum være:

```text id="m8d1pv"
ConnectionStrings:DefaultConnection
Jwt:Key
```

## Hvis noget fejler

Ved databasefejl:

- Kontroller at MySQL Server kører.
- Kontroller port `3306`.
- Kontroller connection string i User Secrets.

Ved JWT/authentication-fejl:

```powershell id="xif90d"
dotnet user-secrets list
```

Kontroller at `Jwt:Key` findes.

Ved manglende packages:

```powershell id="h1ynq3"
dotnet restore
dotnet build
```
