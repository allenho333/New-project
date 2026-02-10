# EF Core Migrations

This folder is intentionally scaffolded for migration files.

Generate the initial migration (network-enabled machine):

```bash
cd /Users/allenhe/Documents/New\ project
dotnet restore src/backend/InterviewShowcase.Api/InterviewShowcase.Api.csproj
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/backend/InterviewShowcase.Api/InterviewShowcase.Api.csproj --output-dir Migrations
dotnet ef database update --project src/backend/InterviewShowcase.Api/InterviewShowcase.Api.csproj
```

After generating migrations, commit the resulting files in this directory.
