# 🚀 MVP Backend API

Welcome to my backend API for the MVP system. Engineered for high performance, enterprise-grade scalability, and long-term maintainability.

## 🛠 Tech Stack

- **Framework:** .NET 10
- **Architecture:** Clean Architecture (CA)
- **ORM:** Entity Framework Core
- **Database:** SQL Server / PostgreSQL

## 🏗 Architecture Overview

This project rigorously adheres to **Clean Architecture** principles, enforcing a strict separation of concerns through four decoupled layers. Dependencies always flow inwards toward the Domain.

1. **Domain:** The core of our system. Contains entities, value objects, domain events, and domain exceptions. It has absolutely **zero** dependencies on external frameworks or the database.
2. **Application:** The brains of the operation. Contains business use cases, DTOs, interfaces for external services (e.g., repositories), and handles command/query validation.
3. **Infrastructure:** The implementation details. Contains the Entity Framework Core `DbContext`, repository implementations, external API clients, and identity management mechanisms.
4. **WebAPI:** The presentation layer. Exposes endpoints (Minimal APIs / Controllers), processes HTTP requests, and acts exclusively as the composition root for Dependency Injection.

## 🧠 Key Decisions & Trade-offs

### Why .NET 10?
.NET 10 provides massive performance improvements, highly optimized Minimal APIs, and leverages the expressive power of C# 14. The maturity of the ecosystem allows us to build a high-throughput, low-latency API without sacrificing developer experience or security.

### Clean Architecture: Maintainability vs. Initial Overhead
While Clean Architecture introduces more upfront boilerplate compared to a traditional monolithic layer approach, it prevents technical debt. 
* **Trade-off:** The higher initial cognitive load pays off by guaranteeing that our core business rules remain completely isolated from UI, database, or framework churn. This ensures the codebase scales predictably as new developers join the team.

### Architectural Simplicity: Injected Services vs. MediatR
We deliberately opted for **Directly Injected Application Services** via standard interfaces instead of utilizing MediatR for pipeline routing.
* **Trade-off:** While MediatR provides elegant decoupling and CQRS out-of-the-box, direct service injection avoids "magic" routing. We prioritize a clear, traceable call stack that is instantly navigable in the IDE. This decision drastically reduces debugging time and keeps dependencies explicit.

### Data Access: Entity Framework Core
We utilize **EF Core** as our ORM. Migrations are exclusively managed within the Infrastructure layer.
* **Trade-off:** While EF Core adds an abstraction overhead compared to raw SQL, the productivity gains, strongly-typed LINQ capabilities, and built-in tracking outweigh the minor raw performance costs. For critical read-heavy paths, we design our repositories to easily drop down to Dapper or raw SQL when strictly necessary.

### Personal Philosophy: Readability & Strongly Typed over Brevity
"Code is read far more often than it is written."
We prioritize comprehensive, explicit naming and **strongly-typed structures** over clever, shortened syntax. You will find that we prefer explicit result object patterns rather than using exceptions for control flow. Our focus is to make the codebase self-documenting, safe, and easily understandable for any engineer to grasp at a glance.

## 🚀 How to Run

1. **Ensure Prerequisites:** Install the .NET 10 SDK.
2. **Setup Database:** Update your connection string in `appsettings.Development.json`.
3. **Run Migrations:** 
   ```bash
   dotnet ef database update --project Infrastructure --startup-project WebAPI
   ```
4. **Start the API:**
   ```bash
   dotnet run --project WebAPI
   ```
5. **Explore API:** Navigate to `https://localhost:44329/scalar/v1` to view the endpoint documentation.