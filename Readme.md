# Shipment Management API

A production-ready .NET 10 Web API for managing shipments with multi-tenant support, built with clean architecture principles.

## Features

- **Full CRUD Operations** - Create, Read, Update, and Delete shipments
- **Multi-Tenant Architecture** - Complete tenant isolation at the database level
- **Optimistic Concurrency Control** - Version tracking to prevent conflicts
- **Soft Deletes** - Audit trail with full delete tracking
- **Pagination Support** - Efficient data retrieval for large datasets
- **Status Management** - Track shipment lifecycle with validation
- **Swagger Documentation** - Interactive API documentation
- **Health Checks** - Monitor application health
- **Production-Ready Features**:
  - Retry logic for database connections
  - Query optimization with NoTracking
  - Comprehensive error handling
  - Audit logging (CreatedBy, UpdatedBy, DeletedBy)

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8 or later) or [Visual Studio Code](https://code.visualstudio.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for SQL Server)
- [SQL Server Management Studio](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) (optional, for database management)

## Local Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd Lateral-Challenge
```

### 2. Start SQL Server with Docker

```bash
docker-compose up -d
```

**Or** run SQL Server manually:

```powershell
docker run -e "ACCEPT_EULA=Y" `
           -e "MSSQL_SA_PASSWORD=LateralInterview!1" `
           -p 1433:1433 `
           --name shipment-sql `
           -v shipment_sql_data:/var/opt/mssql `
           -d mcr.microsoft.com/mssql/server:2022-latest
```

### 3. Configure Connection String using dotnet user-secrets (easier config)

```dotnet
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ShipmentDb;User Id=sa;Password=LateralInterview!1;TrustServerCertificate=True;"
```

### 4. Run Database Migrations

Open Package Manager Console in Visual Studio, select Start-up Project the Client and the Default Project from the console 5_Database\Database:

```nuget
Add-Migration InitialCreate
Update-Database
```

### 5. Run the Application

#### Using Visual Studio:
1. Open the solution file (`.sln`) in Visual Studio
2. Set the Client project as the startup project
3. Press `F5` or click "Start Debugging"

The API will start at:
- **HTTPS**: `https://localhost:7186`
- **HTTP**: `http://localhost:5148`

### 6. Access Swagger Documentation

Navigate to: `https://localhost:7186/swagger`

## Running Tests

### Using Visual Studio:
1. Open Test Explorer (`Test > Test Explorer`)
2. Click "Run All Tests"

### Using .NET CLI:
```bash
dotnet test
```

## API Endpoints

### Shipments

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/shipments/{id}` | Get a shipment by ID |
| GET | `/api/shipments` | Get all shipments (paginated) |
| GET | `/api/shipments/by-status/{status}` | Get shipments by status |
| POST | `/api/shipments` | Create a new shipment |
| PUT | `/api/shipments/{id}` | Update a shipment |
| PATCH | `/api/shipments/{id}/status` | Update shipment status |
| DELETE | `/api/Shipments/{id}` | Delete a shipment

### Health Check

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Application health check |

## Shipment Statuses

- `Created` - Initial status (0)
- `InTransit` - Shipment is on the way (1)
- `Delivered` - Shipment completed (terminal state) (2)
- `Cancelled` - Shipment cancelled (terminal state) (3)

**Note**: Once a shipment is `Delivered` or `Cancelled`, its status cannot be changed.

## Multi-Tenant Support

The API supports multi-tenancy through the `SelectedTenantId` header:

<img width="1269" height="645" alt="image" src="https://github.com/user-attachments/assets/61ff69fc-48f0-40c7-9e49-f08601b6d477" />

All operations are automatically scoped to the tenant specified in the header.

## Architecture

The solution follows Clean Architecture principles with clear separation of concerns:

```
├── Client.API/          # API Controllers, DTOs, Presentation Layer
├── Manager/             # Business Logic, Validation, Use Cases
├── Resources/           # Data Access, Repository Pattern
├── Database/            # Entity Framework, Database Models
├── Infrastructure/      # Cross-cutting Concerns, Interfaces
└── ShipmentAPI.Tests/   # Unit and Integration Tests
```

### Layer Responsibilities

- **Client.API**: HTTP endpoints, request/response mapping, API contracts
- **Manager**: Business rules, validation, orchestration
- **Resources**: Database queries, data persistence
- **Database**: EF Core context, entity configurations
- **Infrastructure**: Shared interfaces, base classes, services

## Configuration

### Database Connection Resilience

The application includes built-in retry logic:
- **Max Retry Count**: 5
- **Max Retry Delay**: 10 seconds
- Automatic retry on transient failures

## Troubleshooting

### Database Connection Issues

**Problem**: Cannot connect to SQL Server

**Solutions**:
1. Ensure Docker container is running: `docker ps`
2. Check SQL Server is accepting connections: `docker logs sqlserver`
3. Verify connection string in `appsettings.json`
4. Ensure port 1433 is not blocked by firewall

### Migration Issues

**Problem**: Migrations fail to apply

**Solutions**:
1. Drop the database and rerun: `dotnet ef database drop`
2. Remove migration files and recreate: `dotnet ef migrations remove`
3. Check database permissions for the user

### Swagger Not Loading

**Problem**: `/swagger` endpoint returns 404

**Solutions**:
1. Ensure you're running in Development mode
2. Check `launchSettings.json` for correct environment

## Database Schema

### Shipments Table

| Column | Type | Description |
|--------|------|-------------|
| ID | uniqueidentifier | Primary key |
| TrackingNumber | nvarchar(50) | Unique tracking number |
| RecipientName | nvarchar(255) | Recipient name |
| Status | int | Shipment status enum |
| Tenant | nvarchar(255) | Tenant identifier |
| CreatedAt | datetime2 | Creation timestamp |
| CreatedBy | uniqueidentifier | User who created |
| UpdatedAt | datetime2 | Last update timestamp |
| UpdatedBy | uniqueidentifier | User who updated |
| IsDeleted | bit | Soft delete flag |
| DeletedAt | datetime2 | Deletion timestamp |
| DeletedBy | uniqueidentifier | User who deleted |
| Version | rowversion | Concurrency token |

### Indexes

- `IX_Shipments_TrackingNumber` (Unique)
- `IX_Shipments_Status`
- `IX_Shipments_Tenant_Status_IsDeleted` (Filtered)

### Performance Tests
<img width="1153" height="547" alt="perf_test1" src="https://github.com/user-attachments/assets/be6c45ac-adae-4f74-8cb6-1cd988997549" /><img width="1056" height="380" alt="perf_test2" src="https://github.com/user-attachments/assets/af191488-e0e2-4393-80bb-0dfc13f41bdc" /><img width="991" height="405" alt="perf_test3" src="https://github.com/user-attachments/assets/2a00b791-6711-4efe-a819-49dc0553ab4b" /><img width="1014" height="383" alt="perf_test4" src="https://github.com/user-attachments/assets/8b20716a-5ace-4c39-8730-799a83ff3b76" /><img width="951" height="386" alt="perf_test5" src="https://github.com/user-attachments/assets/a55067b8-c822-4173-af9e-2edc631744b5" />

## Support

For issues and questions:
- Contact: costelasdenis@gmail.com

---

**Built with ❤️ using .NET 10 and Clean Architecture principles**
