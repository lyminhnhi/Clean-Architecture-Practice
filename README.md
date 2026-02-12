# CodeLeap Assessment – E-commerce API

## Overview

This project is a technical assessment submission for CodeLeap.  
It is a RESTful API built with C# and .NET Core that implements a basic e-commerce backend with authentication and product management.

The project demonstrates:

- Clean architecture design  
- JWT-based authentication with refresh tokens  
- Proper RESTful API practices  
- Input validation and error handling  
- Unit and integration testing  
- Automatically generated OpenAPI documentation  

## Technology Stack

- .NET 8  
- ASP.NET Core Web API  
- Entity Framework Core  
- SQL Server (LocalDB)  
- JWT Authentication  
- Swagger / Swashbuckle  
- xUnit for testing  

## Architecture

The solution follows Clean Architecture principles:

- CodeLeap.API – Presentation Layer (Controllers, Middleware)  
- CodeLeap.Application – Business Logic (Services, DTOs)  
- CodeLeap.Domain – Core Entities and Interfaces  
- CodeLeap.Infrastructure – Data Access and EF Core  
- CodeLeap.Tests – Unit and Integration Tests  

### Design Decisions

- Clean separation of concerns for maintainability  
- Repository pattern for database access  
- JWT with refresh tokens for secure authentication  
- DTOs to protect domain models  
- Centralized error handling  
- Swagger for API documentation  

## Implemented Functionalities

### Authentication

- User registration  
- User login  
- JWT token generation  
- Refresh token mechanism  
- Logout with token invalidation  

### Product Management

- Create product  
- Update product  
- Delete product  
- List products  
- Search products by name  

### Security Features

- Password hashing  
- JWT-protected endpoints  
- Refresh token storage and revocation  
- Input validation  

## OpenAPI Documentation

All APIs are fully documented using Swagger.

After running the project, access:

https://localhost:{port}/swagger

### Using Authentication in Swagger

1. Call the endpoint: /api/auth/login  
2. Copy the returned JWT token  
3. Click the Authorize button in Swagger  
4. Enter the token in the format:

Bearer {your_token}

All protected endpoints will then be accessible.

## Database

The application uses SQL Server with the following entities:

- User  
- Product  
- RefreshToken  

Data validation is applied using:

- Entity configurations  
- Data annotations  
- Application-level validation  

## How to Build and Run

### Prerequisites

- .NET 8 SDK  
- SQL Server or LocalDB  

### Steps to Run

1. Clone the repository:

git clone <repository-url>

2. Navigate to the project folder:

cd codeleap-assessment

3. Apply database migrations:

dotnet ef database update

4. Run the API:

dotnet run --project CodeLeap.API

5. Open Swagger UI:

https://localhost:{port}/swagger

## Running Tests

To execute all tests, run:

dotnet test

The project includes:

- At least one unit test  
- At least one integration test  

## API Endpoints

### Authentication

- POST /api/auth/register – Create a new user  
- POST /api/auth/login – Login and get JWT token  
- POST /api/auth/refresh – Refresh access token  
- POST /api/auth/logout – Logout and invalidate session  

### Products

- GET /api/products – Get all products  
- GET /api/products?search= – Search products  
- POST /api/products – Create product  
- PUT /api/products/{id} – Update product  
- DELETE /api/products/{id} – Delete product  

## Validation Rules

### User Validation

- Email must be in valid format  
- Password must:  
  - Be at least 8 characters long  
  - Contain at least one uppercase letter  
  - Contain at least one lowercase letter  
  - Contain at least one number  

### Product Validation

- Name is required  
- Image URL must be valid  

## Error Handling

The API uses standard HTTP status codes:

- 400 – Bad Request (validation errors)  
- 401 – Unauthorized  
- 404 – Resource not found  
- 500 – Internal server error  

All errors are logged appropriately.

## Docker Support

The project can be easily run using Docker and Docker Compose.

### Prerequisites
- Docker Desktop installed

### How to run

From the root folder, execute:

docker compose up --build

Then open:

http://localhost:5000/swagger

### Services

- ASP.NET Core Web API: http://localhost:5000  
- SQL Server runs inside container and requires no local setup

### Environment Variables

The following variables are used in Docker:

- ConnectionStrings__DefaultConnection  
- Jwt__Key  
- Jwt__Issuer  
- Jwt__Audience

These are preconfigured in docker-compose.yml for easy testing.

## Author

Minh Nhi  
