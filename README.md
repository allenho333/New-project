# Interview Showcase App

A full-stack starter project designed to demonstrate software engineering skills across `.NET`, `React`, `PostgreSQL`, `Docker`, and `AWS`.

## Repository structure

- `src/backend/InterviewShowcase.Api`: ASP.NET Core API with EF Core and JWT authentication
- `src/frontend`: React + TypeScript dashboard frontend with login/register flow
- `tests/InterviewShowcase.Api.Tests`: xUnit test project (password hashing tests)
- `infra/terraform`: AWS IaC starter modules and environment composition
- `.github/workflows/ci.yml`: CI pipeline for backend build/tests and frontend build

## Features implemented

- JWT auth (`/api/auth/register`, `/api/auth/login`)
- Protected CRUD API routes for projects and tasks plus dashboard summary
- EF Core DbContext + PostgreSQL schema initialization (`EnsureCreated`)
- Seeded demo user and sample data for immediate walkthrough
- React UI for sign-in/register and authenticated metrics dashboard
- Design-time DbContext factory and migrations folder scaffold

## Local run

### 1) Start dependencies (PostgreSQL)

```bash
cd /Users/allenhe/Documents/New\ project
docker compose up postgres -d
```

### 2) Run API

```bash
cd /Users/allenhe/Documents/New\ project
dotnet restore src/backend/InterviewShowcase.Api/InterviewShowcase.Api.csproj
dotnet run --project src/backend/InterviewShowcase.Api/InterviewShowcase.Api.csproj
```

API URL: `http://localhost:8080` (Docker) or launch-profile URL when running directly.

### 3) Run web app

```bash
cd /Users/allenhe/Documents/New\ project/src/frontend
npm install
npm run dev
```

Open `http://localhost:5173`.

### 4) Demo credentials

- Email: `demo@interview.dev`
- Password: `Demo123!`

## Full stack in containers

```bash
cd /Users/allenhe/Documents/New\ project
docker compose up --build
```

- Web: `http://localhost:3000`
- API: `http://localhost:8080`
- PostgreSQL: `localhost:5432`

## Deploy frontend to Vercel

This repository is full-stack. Vercel is best used for the React frontend in `src/frontend`.

1. Push this repo to GitHub.
2. In Vercel, create a new project from that repo.
3. Set **Root Directory** to `src/frontend`.
4. Vercel should detect Vite automatically. If needed:
   - Build Command: `npm run build`
   - Output Directory: `dist`
5. Add environment variable:
   - `VITE_API_BASE_URL` = your deployed backend URL (for example, AWS/Render/Railway API URL)
6. Deploy.

CLI alternative:

```bash
cd /Users/allenhe/Documents/New\ project/src/frontend
npm install -g vercel
vercel
vercel --prod
```

The SPA rewrite fallback is configured in `src/frontend/vercel.json`.

## Deploy backend + database on Render

This repo includes a Render blueprint at `/Users/allenhe/Documents/New project/render.yaml`.

1. Push your repository to GitHub.
2. In Render, click **New** -> **Blueprint** and select this repo.
3. Render will create:
   - Web service: `interview-showcase-api` (Docker)
   - PostgreSQL: `interview-showcase-db`
4. After first deploy, open the web service environment variables and set:
   - `CORS_ALLOWED_ORIGINS` to your Vercel domain
   - Example: `https://your-app.vercel.app`
   - For multiple domains, use comma-separated values
5. Confirm health endpoint:
   - `https://<your-render-service>/health`

Then update Vercel:

1. In Vercel project settings, set:
   - `VITE_API_BASE_URL=https://<your-render-service>`
2. Redeploy Vercel (`vercel --prod` or via dashboard).

## EF Core migration commands

```bash
cd /Users/allenhe/Documents/New\ project
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/backend/InterviewShowcase.Api/InterviewShowcase.Api.csproj
dotnet ef database update --project src/backend/InterviewShowcase.Api/InterviewShowcase.Api.csproj
```

## Suggested interview roadmap

1. Add refresh tokens + secure cookie flow
2. Add role-based authorization and audit logging
3. Replace `EnsureCreated` seeding with formal migrations + seed script
4. Add integration tests for auth and ownership checks
5. Implement full AWS resources in `infra/terraform` (RDS, Cognito, ECS/App Runner, S3/CloudFront)
