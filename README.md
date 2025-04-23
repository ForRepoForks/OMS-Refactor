# Order Management System

## Project Introduction

A simple order management system API for retailers, built with .NET 8 and PostgreSQL. All services are orchestrated via a single Docker Compose file at the project root. The project follows Test-Driven Development (TDD), a clear commit strategy, and is continuously integrated and deployed (CI/CD).

## Project Status

### 🌀 Perpetual Work-in-Progress
> **Heads up!**  
> This project is under continuous evolution—there’s always more to refactor, improve, and build. The TODO list never ends!  
> See [docs/REFACTOR_TODO.md](./docs/REFACTOR_TODO.md), [docs/FEATURES.md](./docs/FEATURES.md), and [docs/PLANNED_WORK.md](./docs/PLANNED_WORK.md) for the real roadmap, known issues, and technical debt.

## Business Requirements

### Functional
- Create new products (name, price).
- List products, with search by name.
- Apply discounts to products (percentage and minimum quantity for discount).
- Create orders with a list of products and quantities.
- List all orders.
- Retrieve an order invoice (shows product details, discounts, amounts, and total).
- Retrieve reports for each discounted product (shows product name, discount %, number of orders, and total amount ordered).

### Non-Functional
- Uses a persistence layer (Postgres).
- API rejects invalid requests.
- Prerequisites and launch steps are in the README.
- Solution is in a git repository.
- Implemented in .NET LTS (currently .NET 8).

### Bonus Features
- [x] Automated tests
- [x] RESTful API
- [~] Performance considerations (basic pagination, scalable DB; further optimization possible)
- [x] API documentation generated from code (Swagger)
- [x] Containerization/deployment (Docker Compose)
- [~] Structured code (monolith, but organized by feature; could further modularize)
- [x] Continuous integration (CI/CD)
- [x] Progress tracked via commit strategy and docs
- [x] Comments and rationale in code/docs

**Legend:**
- `[x]` Complete
- `[~]` Partial / In progress
- `[ ]` Not started

## Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Docker Compose](https://docs.docker.com/compose/)

### Setup
1. **Clone the repository:**
   ```sh
   git clone <your-repo-url>
   cd OrderManagementSystem
   ```
2. **Start the API and database using Docker Compose:**
   ```sh
   docker compose up --build
   ```
   - API: http://localhost:8080
   - PostgreSQL: localhost:5432 (user: `omsuser`, password: `omspassword`, db: `omsdb`)

### Quick Links
- Run the API & DB:  
  `docker compose up --build`
- Clean build artifacts:  
  `dotnet clean`
- Format code to style conventions:  
  `dotnet format`
- Run migrations:  
  `dotnet ef database update`
- Run tests:  
  `dotnet test`
- API docs:  
  [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html)

> **Note:** If you find yourself needing to run `dotnet clean` to fix build issues, you’re not alone—this is a known pain point and a sign that our build process could use improvement.

## Development Workflow

### Run
- Start the API and database using Docker Compose: `docker compose up --build`

### Test
- Run automated tests: `dotnet test`

### Format
- Format code to style conventions: `dotnet format`

### Migrations
- Run migrations: `dotnet ef database update`

## API Overview

### Endpoints
- **Products**
  - `POST /api/products` — Create a product
  - `GET /api/products` — List products (supports search & pagination)
  - `PUT /api/products/{id}/discount` — Apply discount to product
- **Orders**
  - `POST /api/orders` — Create an order
  - `GET /api/orders` — List orders (supports pagination)
  - `GET /api/orders/{id}/invoice` — Get order invoice
- **Reports**
  - `GET /api/reports/discounted-products` — Discounted product report (implemented in `OrdersController`)

### Pagination
All list endpoints (`GET /api/products`, `GET /api/orders`) support pagination:
- Query parameters: `page` (default: 1), `pageSize` (default: 10, max: 100)
- Response is wrapped in a `PagedResult<T>`:
  ```json
  {
    "items": [ ... ],
    "totalCount": 123,
    "page": 1,
    "pageSize": 10
  }
  ```

### Error Handling
- The API uses a custom `ArgumentExceptionMiddleware` for consistent error responses. Expect clear error messages for invalid requests.

### Open API
- The API is open (no authentication/authorization required). All endpoints are public by default.

## Project Structure
- `OrderManagementSystem.API/` — Main API project (controllers, models, services, data, DTOs, migrations, middleware, mapping)
- `OrderManagementSystem.Tests/` — Automated tests (unit and integration)
- `docker-compose.yml` — Orchestrates API and database
- `docs/` — Documentation and supporting materials (roadmap, technical debt, planned work)

> **Note:** The `docs/` folder (and even the repo itself) is intentionally a bit of a mess—TODO lists, plans, and ideas are scattered across multiple files. There is no single source of truth. Check around for progress, open tasks, and context!

## Contributing
- Contributions, bug reports, and suggestions are always welcome! See the `docs/` folder for roadmap, planned work, and technical debt.
- If you’re adding tests, check out `OrderManagementSystem.Tests/TestHelpers/DbContextTestHelper.cs` for setup help.
- For code style and commit conventions, follow the existing patterns and see open PRs for examples.

## Troubleshooting
- Some warnings about nullable reference types or EF Core version conflicts may appear during build/test. These do not affect functionality but can be addressed for code quality.
- If you change data models, always update and apply EF Core migrations:
  ```sh
  dotnet ef migrations add <MigrationName>
  dotnet ef database update
  ```

## Additional Notes
- The project is designed for rapid MVP delivery and can be extended with additional features, validation, and performance optimizations as needed.
- **Design Choice:** The code structure is a simple monolith by design to enable fast iteration and delivery of a minimum viable product (MVP). For larger-scale or long-term projects, introducing layered or modular architecture (e.g., NTier, Onion) is recommended to improve maintainability and scalability.
- The `docs/` folder contains supporting documentation.
- See `FEATURES.md` for progress tracking and development roadmap.

## Default Credentials (Development Only)
- The default database credentials (`omsuser` / `omspassword`) and connection strings provided in `docker-compose.yml` and `appsettings.json` are **for local development and testing only**.
- **Never use these credentials in production!**
- For production deployments:
  - Always generate strong, unique database usernames and passwords.
  - Store secrets in environment variables, secret managers, or secure configuration providers (e.g., Azure Key Vault, AWS Secrets Manager).
  - Do not commit production secrets or credentials to source control.
  - Update your deployment pipeline to inject secrets securely at runtime.
  - Review your cloud provider's best practices for secret management.

See the [Docker Compose file](./docker-compose.yml) and [appsettings.json](./OrderManagementSystem.API/appsettings.json) for where these defaults are set.

## API Documentation
- Swagger UI is available at [http://localhost:8080/swagger/index.html](http://localhost:8080/swagger/index.html) when running via Docker Compose (default setup).
- **Note:** If running in Visual Studio or with a different Docker port mapping, the port may vary (e.g., https://localhost:32773/swagger/index.html). Always check your terminal or Visual Studio output for the correct port.

## Troubleshooting & Warnings
- Some warnings about nullable reference types or EF Core version conflicts may appear during build/test. These do not affect functionality but can be addressed for code quality.
- If you change data models, always update and apply EF Core migrations:
  ```sh
  dotnet ef migrations add <MigrationName>
  dotnet ef database update
  ```

## Error Handling
- The API uses a custom `ArgumentExceptionMiddleware` for consistent error responses. Expect clear error messages for invalid requests.

## Contributing
- Contributions, bug reports, and suggestions are always welcome! See the `docs/` folder for roadmap, planned work, and technical debt.
- If you’re adding tests, check out `OrderManagementSystem.Tests/TestHelpers/DbContextTestHelper.cs` for setup help.
- For code style and commit conventions, follow the existing patterns and see open PRs for examples.

## Additional Notes
- The project is designed for rapid MVP delivery and can be extended with additional features, validation, and performance optimizations as needed.
- **Design Choice:** The code structure is a simple monolith by design to enable fast iteration and delivery of a minimum viable product (MVP). For larger-scale or long-term projects, introducing layered or modular architecture (e.g., NTier, Onion) is recommended to improve maintainability and scalability.
- The `docs/` folder contains supporting documentation.
- See `FEATURES.md` for progress tracking and development roadmap.