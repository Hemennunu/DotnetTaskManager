# TaskFlow — .NET Task Manager API

A small, production-focused task management API built with ASP.NET Core, Entity Framework Core, Identity and JWT authentication. It provides user management, role-based access (Admin/User), task CRUD operations, migrations and automatic seeding.

**Tech stack:** .NET 8, ASP.NET Core, Entity Framework Core, PostgreSQL (Npgsql), ASP.NET Identity, JWT

**Quick links**
- Project entry: [TaskFlow.API/Program.cs](TaskFlow.API/Program.cs)
- App settings: [TaskFlow.API/appsettings.json](TaskFlow.API/appsettings.json)
- Db context & seeder: [TaskFlow.Infrastructure/Data/ApplicationDbContext.cs](TaskFlow.Infrastructure/Data/ApplicationDbContext.cs), [TaskFlow.Infrastructure/Data/DatabaseSeeder.cs](TaskFlow.Infrastructure/Data/DatabaseSeeder.cs)
- Auth service: [TaskFlow.Infrastructure/Services/AuthService.cs](TaskFlow.Infrastructure/Services/AuthService.cs)

**Contents**
- **Project Overview**
- **Prerequisites**
- **Configuration**
- **Run locally**
- **Database, migrations & seeding**
- **API Reference (endpoints + examples)**
- **Development notes**
- **Contributing**

**Project Overview**

TaskFlow is an API-only backend that manages tasks and users. Key responsibilities:
- Authentication using JWT.
- Role-based authorization (`Admin` and `User`).
- Tasks: create (Admin), list, view, update status (assigned user), delete.
- Users: list (Admin) and basic user management via Identity.

**Prerequisites**
- .NET SDK 8 or later.
- PostgreSQL (or change connection to your preferred provider).
- (Optional) `dotnet-ef` CLI for working with migrations: `dotnet tool install --global dotnet-ef`

**Configuration**
- Update the connection string and JWT settings in [TaskFlow.API/appsettings.json](TaskFlow.API/appsettings.json).

Example connection string (Postgres):

```bash
Host=localhost;Database=TaskFlow;Username=postgres;Password=your_password
```

JWT settings can be found under the `Jwt` section in the same file. The repository includes a default development JWT key — replace it for any non-local deployment.

**Run locally**
1. Restore and build:

```bash
dotnet restore
dotnet build
```

2. Run the API (from the solution root):

```bash
dotnet run --project TaskFlow.API
```

3. After the app starts you can open Swagger UI at `https://localhost:{port}/swagger` (default port depends on your environment).

**Database, migrations & seeding**
- The app uses EF Core with Npgsql (Postgres provider). On startup the app calls `DatabaseSeeder.SeedAsync`, which applies pending migrations and seeds roles and a default admin user.
- Default seeded admin (from DatabaseSeeder):
	- Email: `admin@taskflow.com`
	- Password: `Admin@123`

If you prefer manual migrations:

```bash
dotnet ef migrations add <Name> --project TaskFlow.Infrastructure --startup-project TaskFlow.API
dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.API
```

**API Reference**

Authentication
- POST /api/auth/login — body: `{ "userName": "...", "password": "..." }` — returns `{ Token: "<jwt>" }`.
- POST /api/auth/register — registers a new user.
- POST /api/auth/seed-admin — creates an `admin` user if missing (convenience endpoint).

Users
- GET /api/users — Admin only. Returns list of users.

Tasks (require Authorization: Bearer <token>)
- GET /api/tasks — returns tasks. Admins see all; users see tasks assigned to them.
- GET /api/tasks/{id} — get a single task (authorization checked).
- POST /api/tasks — create a task (Admin only). Body: `CreateTaskDto` (Title, Description, Priority, AssignedUserId).
- PUT /api/tasks/{id}/status — update only the status (assigned user).

Example: login and create task (curl)

```bash
# 1) Login
curl -X POST https://localhost:5001/api/auth/login \
	-H "Content-Type: application/json" \
	-d '{"userName":"admin@taskflow.com","password":"Admin@123"}'

# 2) Use the returned token for subsequent requests (replace <TOKEN>)
curl -X POST https://localhost:5001/api/tasks \
	-H "Content-Type: application/json" \
	-H "Authorization: Bearer <TOKEN>" \
	-d '{"title":"Sample task","description":"Do something","priority":1,"assignedUserId":"<user-id>"}'
```

Request/response DTOs live under [TaskFlow.Application/DTOs](TaskFlow.Application/DTOs).

**Development notes**
- CORS is configured with the `AllowReactApp` policy to allow `http://localhost:3000`.
- Identity tables and Roles are created by migrations; `ApplicationDbContext` seeds `Admin` and `User` roles in `OnModelCreating`.
- Database seeding runs automatically on app startup via `DatabaseSeeder.SeedAsync` called in [TaskFlow.API/Program.cs](TaskFlow.API/Program.cs).
- Services and repositories are registered in DI in Program.cs: `ITaskRepository`, `IUserRepository`, `IAuthService`.

**Testing**
- The project has no automated tests included. To manually verify: run the app and exercise endpoints via Swagger or `curl`.

**Contributing**
- Open an issue or PR.
- Follow the existing code structure: `TaskFlow.Domain` (entities/enums), `TaskFlow.Application` (DTOs & interfaces), `TaskFlow.Infrastructure` (data, repos, services), `TaskFlow.API` (controllers + startup).

**License**
- This repository does not include a license file. Add an appropriate license if you plan to publish or reuse the code.

---





