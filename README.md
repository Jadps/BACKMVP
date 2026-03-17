# SaaS MVP Backend API

RESTful API de alto rendimiento construida para un MVP de Software as a Service (SaaS). Implementa principios de **Clean Architecture** para garantizar la separación de responsabilidades, mantenibilidad y escalabilidad.

🚀 **Demo en Vivo (Health Check):** [https://backmvp.onrender.com/api/health](https://backmvp.onrender.com/api/health)
🌐 **Frontend App:** [https://frontmvp-ashy.vercel.app]

## ⚙️ Stack Tecnológico

* **Framework:** .NET 10 (ASP.NET Core Web API)
* **Lenguaje:** C# 14
* **Base de Datos:** PostgreSQL (Supabase) via Entity Framework Core
* **Autenticación:** ASP.NET Core Identity + JWT (JSON Web Tokens)
* **Seguridad:** Antiforgery Tokens (XSRF), Rate Limiting, CORS estricto
* **Background Jobs:** Hangfire
* **Infraestructura/Despliegue:** Render (Dockerized)

## 🏗️ Arquitectura (Clean Architecture)

El proyecto está dividido en 4 capas principales:

1.  **Domain:** Entidades centrales (`User`, `Tenant`, `Role`, `Module`), Enums y excepciones de dominio. Sin dependencias externas.
2.  **Application:** Casos de uso, interfaces (contratos) y DTOs.
3.  **Infrastructure:** Implementaciones de las interfaces, configuración de Entity Framework (`ApplicationDbContext`), servicios de Identity y Hangfire.
4.  **WebAPI:** Controladores REST, Middleware (Manejo global de errores, resolución de Tenant) y configuración de inyección de dependencias (`Program.cs`).

## 🛠️ Instalación y Configuración Local

### Requisitos Previos
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* Instancia de PostgreSQL (Local o Docker)

### Variables de Entorno
Crea un archivo `appsettings.Development.json` o configura tus "User Secrets" con las siguientes llaves requeridas:

| Llave | Descripción |
| :--- | :--- |
| `ConnectionStrings:DefaultConnection` | Cadena de conexión a PostgreSQL |
| `Jwt:SecretKey` | Llave HMACSHA256 (Mínimo 32 caracteres) |
| `AdminInitialPassword` | Contraseña para el seeder del administrador inicial |

### Ejecución
1. Clona el repositorio: `git clone https://github.com/Jadps/BACKMVP.git`
2. Restaura las dependencias: `dotnet restore`
3. Aplica las migraciones de base de datos: `dotnet ef database update --project MVP.Infrastructure --startup-project MVP.WebAPI`
4. Ejecuta la aplicación: `dotnet run --project MVP.WebAPI`

La API estará disponible en `https://localhost:44329` (o el puerto configurado en `launchSettings.json`). La documentación OpenAPI (Scalar/Swagger) estará disponible en la ruta raíz durante el desarrollo.