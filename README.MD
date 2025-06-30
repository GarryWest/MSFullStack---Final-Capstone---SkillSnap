# SkillSnap

SkillSnap is a full-stack .NET portfolio and skill management application. It consists of:

- **SkillSnap.Api** – an ASP.NET Core Web API with JWT-secured endpoints, EF Core, Identity, and in-memory caching
- **SkillSnap.Client** – a Blazor WebAssembly client that consumes the API, handles authentication, and displays projects, skills, and user profiles
- **SkillSnap.Shared** – shared model and DTO definitions
- **SkillSnap.Api.Tests** – integration tests for controllers and authorization flows
- **SkillSnap.Client.Tests** – bUnit tests for Blazor components

---

## Features

- JWT-based authentication & role-based authorization
- CRUD for portfolio users, projects, and skills
- Database seeding and EF Core migrations
- IMemoryCache caching for portfolio data
- Blazor WASM client with protected routes and forms
- Automated tests covering API and UI components

---

## Prerequisites

- .NET 7.0 SDK or later
- SQL Server (or modify `appsettings.json` for SQLite/PostgreSQL)
- Node.js & npm (optional, for SPA tooling in Client)
- Visual Studio 2022 / VS Code

---

## Get Started

1. **Clone the repo**

   ```bash
   git clone https://github.com/your-org/SkillSnap.git
   cd SkillSnap
   ```

2. **Configure the database**

   - Open `SkillSnap.Api/appsettings.json`
   - Update `ConnectionStrings:DefaultConnection` to point to your database

3. **Apply migrations & seed data**

   ```bash
   cd SkillSnap.Api
   dotnet ef database update
   # Optionally, call the SeedController or let DatabaseSeeder run on startup
   ```

4. **Run the API**

   ```bash
   dotnet run
   # API listens on https://localhost:5001 by default
   ```

5. **Run the Blazor client**

   ```bash
   cd ../SkillSnap.Client
   dotnet run
   # Client app on https://localhost:5002
   ```

6. **Browse the app**
   - Visit the Blazor client URL
   - Register a new account, log in, and manage your portfolio

---

## Testing

### API Integration Tests

```bash
cd SkillSnap.Api.Tests
dotnet test
```

- Uses `CustomWebApplicationFactory` to spin up an in-memory server
- Covers projects, skills, portfolio access, and JWT auth scenarios

### Blazor Component Tests

```bash
cd SkillSnap.Client.Tests
dotnet test
```

- Uses bUnit and Moq to render components in isolation
- Validates markup, loading states, and event callbacks

---

## HTTP Request Samples

Under `SkillSnap.Api/ApiRequests.http` you’ll find ready-to-use HTTP requests for:

- Authentication (login/register)
- PortfolioUser endpoints (`GET /api/PortfolioUser`, `/api/PortfolioUser/{username}`, `POST /api/PortfolioUser/update`)
- Projects endpoints (`GET/POST /api/Projects`, `/api/Projects/mine`)
- Skills endpoints

Load the file in VS Code with the REST Client extension and define your `@token` variable for authenticated calls.

---

## Folder Structure

```text
├── SkillSnap.Api
│   ├── Controllers
│   ├── Data
│   ├── Migrations
│   ├── Services
│   ├── appsettings.json
│   └── Program.cs
├── SkillSnap.Client
│   ├── Components
│   ├── Layout
│   ├── Pages
│   ├── Services
│   └── Program.cs
├── SkillSnap.Shared
│   └── Models
├── SkillSnap.Api.Tests
├── SkillSnap.Client.Tests
└── SkillSnap.sln
```

---

## Contributing

1. Fork the repo
2. Create a feature branch (`git checkout -b feat/YourFeature`)
3. Commit your changes (`git commit -m "Add your feature"`)
4. Push to your branch (`git push origin feat/YourFeature`)
5. Open a pull request

Please adhere to the existing code style, include tests for new functionality, and update this README as needed.

---

Happy coding! 🚀
