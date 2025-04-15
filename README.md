# Online Code Management System

A web-based system for managing and analyzing code submissions with similarity detection capabilities.

## Features

- User authentication with role-based access (Students, Professors, Administrators)
- Code submission and storage
- Automated code similarity analysis
- Detailed similarity reports for professors
- Secure JWT-based authentication
- Swagger API documentation

## Tech Stack

- .NET 6.0
- Entity Framework Core
- SQL Server
- JWT Authentication
- Swagger/OpenAPI

## Project Structure

```
src/
├── OnlineCodeManagementSystem.Api/
│   ├── Controllers/         # API endpoints
│   ├── Models/             # Domain models
│   ├── Data/               # Database context and configurations
│   ├── Services/           # Business logic and services
│   ├── appsettings.json    # Application configuration
│   └── Program.cs          # Application entry point
└── OnlineCodeManagementSystem.sln
```

## Setup Instructions

1. Prerequisites:
   - .NET 6.0 SDK
   - SQL Server

2. Configuration:
   - Update connection string in `appsettings.json`
   - Update JWT settings in `appsettings.json`

3. Database Setup:
   ```bash
   dotnet ef database update
   ```

4. Run the Application:
   ```bash
   dotnet run
   ```

5. Access Swagger Documentation:
   - Navigate to `https://localhost:5001/swagger`

## API Endpoints

### Authentication
- POST /api/auth/register - Register new user
- POST /api/auth/login - User login

### Submissions
- POST /api/submissions - Submit code
- GET /api/submissions/{id} - Get submission
- GET /api/submissions/similarity/{id} - Get similarity report (Professors only)

## Security

- JWT token-based authentication
- Role-based authorization
- Secure password requirements
- HTTPS enabled

## License

MIT License 