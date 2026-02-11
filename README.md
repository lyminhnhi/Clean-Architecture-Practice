# CodeLeap Assessment

This project is a technical assessment for CodeLeap built with:

- .NET 8 Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- Clean Architecture
- Unit Tests & Integration Tests

---

## Architecture

The solution is organized using Clean Architecture:

- CodeLeap.API – Web API layer  
- CodeLeap.Application – Services, DTOs  
- CodeLeap.Domain – Entities, Interfaces  
- CodeLeap.Infrastructure – EF Core, Repositories  
- CodeLeap.UnitTests  
- CodeLeap.IntegrationTests  

---

## Prerequisites

- .NET 8 SDK
- SQL Server (Local or Express)
- Visual Studio 2022

---

## Database Setup

Update connection string in:

`appsettings.json`

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CodeLeapDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Run migrations:

```
Update-Database
```

---

## Running the project

1. Open solution in Visual Studio  
2. Set **CodeLeap.API** as Startup Project  
3. Press **F5**  
4. Swagger will open automatically  

---

## Authentication

- Register user:  
  POST `/api/auth/register`

- Login:  
  POST `/api/auth/login`

- Use returned JWT token to access Product APIs

---

## Testing

Run unit tests:

```
dotnet test
```

---

## Author

- Candidate: NHI
- Date: 2026