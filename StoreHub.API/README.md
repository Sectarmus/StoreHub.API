# StoreHub.API

StoreHub.API is a comprehensive E-Commerce REST API developed using C# and .NET 10. The project integrates modern software architecture concepts to provide high scalability, performance, and security.

## Features & Architecture

This application is built with modern Web API development practices, including:

*   **Database & ORM:** Powered by PostgreSQL and Entity Framework Core (Code First approach). Employs `AsNoTracking()` for optimal data retrieval operations.
*   **Security & Authentication:** Role-based Authorization secured by JWT (JSON Web Token), with secure password hashing via `BCrypt`.
*   **Data Integrity & Validation:** 
    * End-to-end data validation utilizing `FluentValidation` pipelines.
    * Controlled client-server interactions using the DTO (Data Transfer Object) pattern, mapped efficiently via `AutoMapper`.
    * Financial and order management operations are safely wrapped inside database transactions (`BeginTransactionAsync`) to guarantee data sequence integrity.
*   **Error Handling:** Features a robust, centralized Global Exception Middleware, eliminating redundant try-catch blocks and returning standardized JSON error objects across the application.
*   **Media Management:** Built-in secure file upload service specifically designed for handling product imagery.
*   **Performance:** Uses memory caching strategies (`IMemoryCache`) to reduce repetitive database queries for continuous product and order listings.

## Getting Started

Follow these instructions to run the project locally.

### Prerequisites

- .NET 10 SDK
- PostgreSQL

### Installation

1. Clone the repository to your local machine.
2. Configure your `appsettings.json` file by updating the `DefaultConnection` connection string to point to your PostgreSQL instance.
3. Open your terminal in the `StoreHub.API` directory and run:

```bash
# Clean the project (Optional)
dotnet clean

# Apply entity framework migrations to your database
dotnet ef database update

# Run the application
dotnet run
```

4. Once the application is running, navigate to the local endpoint to view the scalar interactive API documentation and test the routes directly.

---
*Developed by Alper (Sectarmus)*
