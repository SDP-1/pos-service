
# pos-service

## Description

`pos-service` is a point-of-sale back-end built with ASP.NET Core Web API. It provides APIs for managing inventory, items, customers, suppliers, orders, contacts, users, roles, permissions, shop settings, backups, and printing. The application uses Entity Framework Core with MySQL, JWT authentication, AutoMapper, and a global exception handling pipeline.

## Features

- RESTful API for POS and admin workflows
- Inventory, items, customers, suppliers, contacts, orders, and user management
- Role and permission-based access control
- JWT bearer authentication
- MySQL database support through Entity Framework Core
- AutoMapper-based entity and DTO mapping
- Runtime database seeding and migrations on startup
- Backup and backup history endpoints
- Report and receipt printing support through FastReport
- CORS configuration for frontend integration

## Installation

### Prerequisites

- .NET 9 SDK
- MySQL server
- A valid database connection string

### Setup Steps

1. Clone the repository.
2. Update `appsettings.json` with your MySQL connection string and JWT secret.
3. Restore dependencies:

```bash
dotnet restore
```

4. Apply database migrations:

```bash
dotnet ef database update
```

5. Run the application:

```bash
dotnet run
```

## Usage

1. Start the API with `dotnet run`.
2. In development, open the OpenAPI endpoint to explore the available routes.
3. Use the authentication flow to obtain a JWT token before calling protected routes.
4. Call the relevant controllers for inventory, customers, orders, users, roles, permissions, backups, and settings as needed.

Example commands:

```bash
dotnet run --environment Development
dotnet ef migrations add AddNewFeature
dotnet ef database update
```

## Screenshots

No screenshots are included in the repository yet. Add API or UI screenshots here if you want to document the system visually.

## Technologies Used

- ASP.NET Core Web API
- .NET 9
- Entity Framework Core
- MySQL
- AutoMapper
- JWT Bearer Authentication
- Microsoft Identity password hashing
- FastReport OpenSource
- CORS

## Contributing

Contributions are welcome. If you plan to extend the project:

1. Create a feature branch.
2. Make your changes with clear, focused commits.
3. Run the project and verify migrations, authentication, and affected endpoints.
4. Submit a pull request with a short description of the change.

## License

 Apache License - Version 2.0
