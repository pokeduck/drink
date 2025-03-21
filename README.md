# Drink

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
└── WebClientApp/ # Vue+Vite+Quasar

````

## Misc
* Inspired by [eShopOnWeb](https://github.com/dotnet-architecture/eShopOnWeb) and [book-exchange-app](https://github.com/dimatrubca/book-exchange-app) using Onion Architecture.