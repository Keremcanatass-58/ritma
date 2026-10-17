# Ritma

Ritma helps appointment-based businesses manage bookings, customers, services, staff, and day-to-day operations from one responsive workspace.

## What it includes

- Appointment management with list and calendar views, status updates, filters, and overlap protection.
- A public booking page with service selection, live availability, and cancellation links.
- Customer search and appointment/payment history.
- Staff, service, and business schedule management.
- Payment status tracking and date-range reports with CSV export.
- Quick WhatsApp and email reminder drafts, plus calendar downloads for bookings.
- JWT authentication, business-scoped data access, request validation, and rate limits on login and public booking.

Ritma currently tracks payment status manually. WhatsApp, SMS, and email actions open drafts; automated delivery and online card processing need provider accounts and credentials.

## Requirements

- .NET 10 SDK
- SQL Server or SQL Server LocalDB

## Configure and run

Copy `appsettings.example.json` to `appsettings.json`, then set a database connection string and JWT settings with .NET user secrets or environment variables. Keep local and production settings out of source control.

```powershell
Copy-Item appsettings.example.json appsettings.json
```

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your SQL Server connection string>"
dotnet user-secrets set "Jwt:Key" "<a random key of at least 32 characters>"
dotnet user-secrets set "Jwt:Issuer" "Ritma"
dotnet user-secrets set "Jwt:Audience" "Ritma"
dotnet tool install --global dotnet-ef
dotnet ef database update
dotnet run --project Ritma.Api.csproj --launch-profile http
```

Open `http://localhost:5019`. In Development, the OpenAPI document is available at `/openapi/v1.json`. Create an owner account from the registration screen to set up a business workspace.

## Current integration scope

- Online booking requests are saved in Ritma and can be managed from the workspace.
- Reminder shortcuts prepare messages for the business to review and send.
- Payments can be recorded and tracked, but no card processor or payment gateway is connected.
- Production hosting, domain, and provider credentials must be configured for a public launch.
