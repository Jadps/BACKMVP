# MVP API

Backend for the MVP system.

This project uses Clean Architecture to keep business logic separated from framework, database, and HTTP concerns.

## Badges

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-14-239120?style=flat&logo=c-sharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-ORM-6DB33F?style=flat)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-336791?style=flat&logo=postgresql&logoColor=white)

![Build Status](https://img.shields.io/github/actions/workflow/status/Jadps/BACKMVP/backend.yml?branch=main&style=flat)
![Coverage](https://img.shields.io/codecov/c/github/Jadps/BACKMVP?style=flat)
![Version](https://img.shields.io/github/v/tag/Jadps/BACKMVP?style=flat)
![License](https://img.shields.io/github/license/Jadps/BACKMVP?style=flat)

## Structure

- **Domain**  
  Entities, value objects, events, and business rules.

- **Application**  
  Use cases, DTOs, interfaces, and validation.

- **Infrastructure**  
  EF Core, repositories, external services, and persistence details.

- **WebAPI**  
  HTTP endpoints and dependency injection setup.

## Design choices

- Direct application services instead of MediatR
- EF Core by default
- Raw SQL or Dapper for read-heavy paths when needed
- Explicit result objects for normal control flow

## Getting started

### Prerequisites

- .NET 10 SDK
- A database connection string

### Setup

1. Update the connection string in `appsettings.Development.json`.

2. Run migrations:

   ```bash
   dotnet ef database update --project Infrastructure --startup-project WebAPI