# Arcane Vault - IT2814 Core Submission

ASP.NET Core Razor Pages collection manager backed by an ASP.NET Core Web API, Entity Framework Core, and SQLite.

## Run the application

1. Open `ArcaneVault.slnx` in Visual Studio 2026.
2. Configure both `ArcaneVault.API` and `ArcaneVault.Web` as startup projects.
3. Start the API and web projects using their HTTPS profiles.
4. Open the web application at `https://localhost:7154`.

The web project calls the API at `https://localhost:7110`. The API automatically creates and seeds `ArcaneVault.API/ArcaneVault.db` if the database is missing.

## Demonstration accounts

The seeded usernames are `staff` and `collector`. Passwords and the JWT signing key are not
stored in source control. To choose the demo passwords and a persistent signing key, configure
these environment variables before starting the API:

```powershell
$env:SeedAccounts__StaffPassword = "<choose-a-password>"
$env:SeedAccounts__UserPassword = "<choose-a-password>"
$env:Jwt__Key = "<at-least-32-random-characters>"
```

Configured demo passwords are applied to both a new and an existing database. If the database
is recreated without configured passwords, secure temporary passwords are generated and shown
in the API console. If `Jwt__Key` is omitted, the API generates an in-memory signing key for that
run. New registrations always receive the User role.

## Core features

- Registration with required-field, email-format, password, and duplicate-email validation
- Secure password hashing and JWT login
- Login/logout navbar state
- Staff-only category List, Create, Details, Edit, and Delete
- Authenticated personal collection List, Create, Details, Edit, and Delete
- Search by item name, item ID, starting quantity, current quantity, owner, or category code
- API-side role and collection-ownership enforcement
- SQLite database with all five required tables and relationships

The separate proposed 20-mark custom feature is intentionally not included here.
