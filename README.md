![logo-text](frontend/public/assets/logo-text.svg)

# Chess: The Anarchy Update

[Terms Of Service](https://anarchychess.org/tos) | [Privacy Policy](https://anarchychess.org/privacy) | [License](LICENSE)

[![Discord](https://img.shields.io/discord/1424365905005121658?label=Discord&logo=discord&style=flat)](https://discord.gg/qnkddndecq)

**Chess: The Anarchy Update** is a chess website that takes all the stupid ideas from [r/AnarchyChess](https://reddit.com/r/anarchychess) and turns them a real, balanced, chess variant.

# Features

- **New Pieces & Rules:** 5 new pieces and 16 new rules.\
  _See the [full guide](https://anarchychess.org/guide) for detailed explanations of all pieces and rules_

- **Analysis:** Analysis board with branching positions and variations, available standalone or from finished games
- **Player Profiles:** Track ratings, game history and progress for each time control
- **Social Features:** Stars, blocks, in-game chat, leaderboards, direct challenges
- **Matchmaking:** Seek a game in any time control, all at once, rated or casual. Your seek is also displayed as an open seek, allowing players to accept it directly without having to go through the pool
- **Daily Quests:** Complete daily quests to climb the leaderboards and build a streak
- **Anarchy Bot:** A chess engine built from scratch. Supports playing against humans on the website with dialog.

# Screenshots

<div>
    <img src="screenshots/long-passant.png" alt="Long Passant" width="400">
    <img src="screenshots/checker.png" alt="Checker" width="400">
    <img src="screenshots/home.png" alt="Home Page" width="400">
    <img src="screenshots/profile.png" alt="Profile Page" width="400">
</div>

# Tech Stack

- **Backend:** C# With ASP.NET Core, structured with Orleans
- **Frontend:** Next.js + Typescript, styled with Tailwind
- **Database & Storage:** Currently configured for PostgreSQL and Azure Blob Storage. Other SQL databases and blob storage providers can be used by installing the appropriate EF Core and FluentStorage packages.

# Anarchy Bot

Anarchy Bot is the AI used on Anarchy Chess, built from scratch.

- **Engine:** `AnarchyChess.Ai` handles move generation, search and position evaluation using bitboards. This is a compelte rewrite of the website's backend move generation, both exist because they serve different purposes, the backend generates moves with with a lot of metadata for frontend animation while the engine needs to compute moves as fast as possible.
- **Service:** `AnarchyChess.Ai.Service` is a thin gRPC wrapper that allows the website to request moves from the engine.
- **Deployment:** The bot runs on a separate VM so it doesn't compete with the backend for resources, and because it can be hosted on a spot VM, which makes computation a lot cheaper.

# Running Locally

First, run the docker compose

```bash
cd docker-compose
docker compose up -d
```

## Database Setup

1. Create a database

```sql
CREATE DATABASE anarchychess;
```

2. Run Orleans SQL Setup Scripts

Run these scripts in order against your database:

```bash
backend/Scripts/Orleans
|- 001-query.sql
|- 002-reminders.sql
|- 003-storage.sql
|- 004-clustering.sql
```

## Backend Setup

1. Navigate to the backend directory

```bash
cd backend/AnarchyChess.Api
```

2. Restore dependencies

```bash
dotnet restore
```

3. Initialize & set secrets

You can set google and discord OAuth to dummy values, but for login to work you need to create an app on https://discord.com/developers/applications and set the OAuth redirect to `https://localhost:7266/api/oauth/discord/callback`. \
JWT secret can be whatever you want as long as it's at least 32 bytes (256 bits) long.

```bash
dotnet user-secrets init

dotnet user-secrets set "AppSettings:Secrets:GoogleOAuth:ClientId" "<client-id>"
dotnet user-secrets set "AppSettings:Secrets:GoogleOAuth:ClientSecret" "<client-secret>"

dotnet user-secrets set "AppSettings:Secrets:DiscordOAuth:ClientId" "<client-id>"
dotnet user-secrets set "AppSettings:Secrets:DiscordOAuth:ClientSecret" "<client-secret>"

dotnet user-secrets set "AppSettings:Secrets:JwtSecret" "<jwt-secret>"

dotnet user-secrets set "AppSettings:Secrets:DatabaseConnString" "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=anarchychess;MaxPoolSize=15;"
dotnet user-secrets set "AppSettings:Secrets:BlobStorageConnString" "azure.blob://emu"
dotnet user-secrets set "AppSettings:Secrets:RedisConnString" "localhost,abortConnect=false"

dotnet user-secrets set "AppSettings:Secrets:TableCheckpointerConnString" "UseDevelopmentStorage=true"
dotnet user-secrets set "AppSettings:Secrets:EventHubConnString" "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"
dotnet user-secrets set "AppSettings:Secrets:EventHubName" "eh1"
dotnet user-secrets set "AppSettings:Secrets:EventHubConsumerGroup" "cg1"
```

4. Run the backend server

```bash
dotnet run
```

## Frontend Setup

1. Navigate to the frontend directory

```bash
cd frontend
```

2. Install dependencies

```bash
npm install
```

3. Setup environment variables

Create a .env file:

```
NEXT_PUBLIC_API_URL="https://localhost:7266"
NEXT_PUBLIC_OAUTH_URL="https://localhost:7266"
NEXT_PUBLIC_ASSETS_URL="http://127.0.0.1:10000/devstoreaccount1/assets"
```

4. Upload pieces and sfx to the azure blob storage emulator (included in the docker compose):

- `/sfx` -> `/assets/sfx`
- `/pieces/full-png` -> `/assets/pieces`
- `/pieces/material-png` -> `/assets/material-pieces`

In production, these assets are not served from next.js public folder.
Instead they are hosted in blob storage behind a CDN because every request to `/public` assets on vercel is counted as an edge request.
Since these files are accessed frequently, hosting them on blob storage saves money.

You also need to make sure the assets blob container has a read public access level, and configure blob container cors settings.

5. Let node trust aspnet dev certificates

```bash
dotnet dev-certs https --trust
dotnet dev-certs https --export-path <path to wherever you'd like to store certs>/localhost.crt --format Pem
```

Then set a permanent environment variable `NODE_EXTRA_CA_CERTS` pointing to the path of the cert

windows:

```cmd
setx NODE_EXTRA_CA_CERTS "C:\path\to\localhost.crt"
```

bash:

```bash
echo 'export NODE_EXTRA_CA_CERTS="$HOME/path/to/localhost.crt"' >> ~/.bashrc
source ~/.bashrc
```

6. Run the development server:

```bash
npm run dev
```

# Running Tests

## Backend

There are 5 test projects:

```bash
backend/Tests
|- AnarchyChess.Api.Unit
|- AnarchyChess.Api.Integration
|- AnarchyChess.Api.Functional
|- AnarchyChess.Ai.Tests
|- AnarchyChess.Ai.Service.Tests
```

To run all backend tests:

```bash
cd backend
dotnet test AnarchyChess.Api.sln
```

## Frontend

The frontend uses Vitest for testing. Run all tests with:

```bash
cd frontend
npm run test
```
