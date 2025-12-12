# Drink
* This is a practice project for learning full-stack web development, built with DotNet Core 10 as the backend framework and Nuxt 3 as the frontend.
* The main feature of this project is to organize group drink orders, and it includes both a frontend (client site) and a backend (admin panel).
* The admin panel UI template uses [AdminLTE v4](https://github.com/ColorlibHQ/AdminLTE).

## Requirement
* .NET Core 10
* Nuxt 3

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
* Inspired by [eShopOnWeb](https://github.com/dotnet-architecture/eShopOnWeb) and [book-exchange-app](https://github.com/dimatrubca/book-exchange-app), with the project structure based on Onion Architecture principles.
