# Drink
This is a practice project for learning full-stack web development, built with DotNet Core 9 as the backend framework and Nuxt 3 as the frontend.

## Requirement
* .NET Core 9.0.2

## Project Structure
````
src/
├── Application/
│   ├── Extensions/
│   └── Mapping/
│
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   └── Interfaces/
│
├── Infrastructure/
│   ├── Data/
│   ├── Helpers/
│   ├── Services/
│   ├── Settings/
│   ├── Migrations/
│   └── Repositories/
│
├── RazorPageAdmin/ # Razor Page
│
├── WebAPI/ # Controller API
│
└── WebClientApp/ # Vue3 + Nuxt + NuxtUI

````

## Misc
* Inspired by [eShopOnWeb](https://github.com/dotnet-architecture/eShopOnWeb) and [book-exchange-app](https://github.com/dimatrubca/book-exchange-app) using Onion Architecture.
