# LEVORN API

A REST API for appointment-based businesses (salons, clinics, studios) to manage customers, services and bookings. Built with **ASP.NET Core** and **Entity Framework Core** on **SQL Server**.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-SQL_Server-CC2927?logo=microsoftsqlserver&logoColor=white)

## Features

- **Customers**: create, list, update and delete customer records
- **Services**: manage the service catalog (duration, price) with soft deactivation
- **Appointments**: book, update and delete appointments
- **Double-booking prevention**: overlapping time slots are rejected with `409 Conflict`
- **Validation**: request DTOs with data annotations (required fields, lengths, phone/email format, ranges)
- **Consistent errors**: `400` for invalid input, `404` for missing resources, `409` for conflicts, and a global exception handler that returns RFC 7807 problem details
- **Migrations**: database schema is versioned with EF Core migrations

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core Web API (.NET 10) |
| ORM | Entity Framework Core |
| Database | SQL Server (LocalDB for development) |
| API docs | OpenAPI (`/openapi/v1.json` in Development) |

## Project Structure

```
Controllers/   HTTP endpoints (Customers, Services, Appointments, Health)
DTOs/          Request models with validation rules
Models/        Domain entities
Data/          LevornDbContext
Migrations/    EF Core schema migrations
```

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/health` | Health check |
| GET | `/api/customers` | List customers |
| GET | `/api/customers/{id}` | Get a customer |
| POST | `/api/customers` | Create a customer |
| PUT | `/api/customers/{id}` | Update a customer |
| DELETE | `/api/customers/{id}` | Delete a customer |
| GET | `/api/services` | List active services |
| POST | `/api/services` | Create a service |
| PUT | `/api/services/{id}` | Update a service |
| PATCH | `/api/services/{id}/deactivate` | Deactivate a service |
| GET | `/api/appointments` | List appointments (with customer) |
| GET | `/api/appointments/{id}` | Get an appointment |
| POST | `/api/appointments` | Book an appointment |
| PUT | `/api/appointments/{id}` | Update an appointment |
| DELETE | `/api/appointments/{id}` | Delete an appointment |

### Example: booking an appointment

```http
POST /api/appointments
Content-Type: application/json

{
  "customerId": 1,
  "startTime": "2026-10-01T14:00:00",
  "endTime": "2026-10-01T15:00:00",
  "notes": "First visit"
}
```

If the slot overlaps an existing appointment, the API responds with:

```json
HTTP/1.1 409 Conflict
{ "message": "Selected time slot is already booked." }
```

## Getting Started

**Requirements:** .NET 10 SDK, SQL Server or LocalDB

```bash
git clone https://github.com/Keremcanatass-58/levorn-api.git
cd levorn-api

# create the database
dotnet tool install --global dotnet-ef
dotnet ef database update

# run
dotnet run
```

The API starts at `http://localhost:5019`. Ready-made requests for every endpoint are in [`levorn-api.http`](levorn-api.http) (runs directly in Visual Studio / VS Code / Rider).

To use a different database, change `ConnectionStrings:DefaultConnection` in `appsettings.json`.

## Roadmap

- [ ] Service layer between controllers and data access
- [ ] Response DTOs instead of returning entities
- [ ] JWT authentication and roles (admin / staff)
- [ ] Unit and integration tests (xUnit + WebApplicationFactory)
- [ ] Docker support and CI pipeline
