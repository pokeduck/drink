# Drink

![.NET Build](https://github.com/pokeduck/drink/actions/workflows/api.yml/badge.svg)
![Web Build](https://github.com/pokeduck/drink/actions/workflows/web.yml/badge.svg)

* This is a practice project for learning full-stack web development, built with .NET 10 as the backend framework and Nuxt 4 as the frontend.
* The main feature of this project is to organize group drink orders, and it includes both a frontend (client site) and a backend (admin panel).

## Tech Stack

**Frontend (Monorepo — pnpm + Turborepo)**
- Client: Nuxt 4 + Nuxt UI
- Admin: Nuxt 4 + Element Plus
- Shared: `web/internal` (utilities, shared interfaces)

**Backend**
- .NET 10, ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL
- JWT Authentication

## Requirements

* .NET 10
* Node.js 22+
* pnpm 10+
* PostgreSQL

## Project Structure

```
drink/
├── api/
│   ├── Domain/               # Entities, Enums, Interfaces
│   ├── Application/          # Services, Requests, Responses, Mappings
│   ├── Infrastructure/       # DbContext, Migrations, EF Extensions
│   ├── User.API/             # User-facing API
│   ├── Admin.API/            # Admin-facing API
│   └── Migrator/             # Migration runner
│
├── web/
│   ├── apps/
│   │   ├── client/           # Nuxt 4 + Nuxt UI (frontend)
│   │   └── admin/            # Nuxt 4 + Element Plus (admin panel)
│   └── internal/             # Shared utilities and interfaces
│
└── specs/                    # Project specifications
```

## Misc

* Inspired by [eShopOnWeb](https://github.com/dotnet-architecture/eShopOnWeb), with the project structure based on Onion Architecture principles.
