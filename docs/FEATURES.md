# Order Management System – Feature Tracking

This document tracks the features and work chunks for the Order Management System project. **All development will follow Test-Driven Development (TDD): write tests before implementing features. Core to everything is a single Docker Compose file at the project root, orchestrating all services and dependencies.** Each chunk is sized for a commit and ~30–60 minutes of work, aiming for a total of 8 hours (~8–12 commits).

---

## 1. Project Setup & Architecture
- [x] Initialize .NET solution and git repository
- [x] Set up project structure (Simple monolithic architecture; all logic in API project, organized by folders)
- [x] Configure database connection (MongoDB or PostgreSQL)
- [x] Add README with prerequisites and launch steps
- [x] Create and maintain a single Docker Compose file at the project root to orchestrate all services (API, database, etc.)

## 2. Product Management
- [x] Write tests for Product entity/model (name, price)
- [x] Create Product entity/model (name, price)
- [x] Write tests for Create Product API endpoint
- [x] Implement Create Product API endpoint
- [x] Write tests for Get/List Products API endpoint
- [x] Implement Get/List Products API endpoint
- [x] Write tests for Product search by name
- [x] Implement Product search by name
- [x] Write tests for Apply Discount to Product (percentage & quantity threshold)
- [x] Implement Apply Discount to Product (percentage & quantity threshold)
- [x] Write tests for product endpoint input validation
- [x] Input validation for product endpoints

## 3. Order Management
- [x] Write tests for Order entity/model (with product list and quantities)
- [x] Create Order entity/model (with product list and quantities)
- [x] Write tests for Create Order API endpoint
- [x] Implement Create Order API endpoint
- [x] Write tests for Get/List Orders API endpoint
- [x] Implement Get/List Orders API endpoint
- [x] Write tests for order endpoint input validation
- [x] Input validation for order endpoints

## 4. Invoices
- [x] Write tests for Order Invoice endpoint (show product name, quantity, discount %, amount, total)
- [x] Implement Order Invoice endpoint (show product name, quantity, discount %, amount, total)

## 5. Reporting
- [x] Write tests for Discounted Product Report endpoint (product name, discount %, number of orders, total amount)
- [x] Implement Discounted Product Report endpoint (product name, discount %, number of orders, total amount)

## 6. Documentation & Quality

---

### Test Isolation & Reliability (Important)

- **Current Workaround:** All integration tests are currently forced to run sequentially (see `OrderManagementSystem.Tests/AssemblyInfo.cs` with `[assembly: CollectionBehavior(DisableTestParallelization = true)]`). This was necessary because parallel test execution caused test failures due to interference and shared state in the PostgreSQL database.
- **Root Cause:** Our integration tests and API use the same PostgreSQL database. When tests run in parallel, they can interfere with each other's data, causing flakiness, "not found" errors, and empty collections.
- **Why This Isn't a True Fix:** Sequential execution only masks the problem. True test isolation requires:
    - A unique database per test run (or per test)
    - Complete cleanup and foreign key handling before each test
    - (Optional) Use of test containers for a fresh DB instance per suite
- **Action Item:** This must be addressed for future scalability and reliability. See commit history and test output for more details.

---
- [x] Add Swagger/OpenAPI API documentation
- [x] Add automated tests (unit/integration)
- [x] Add input validation for all endpoints

## 7. Deployment & CI
- [x] Ensure all services (API, database, etc.) are orchestrated through the single Docker Compose file at the project root
- [x] Add Docker/Docker Compose for containerization
- [x] Set up Continuous Integration (CI)

---

## Commit Strategy
- Each checkbox represents a logical commit.
- For each feature, write tests first, then implement the feature.
- Mark tasks as complete as you progress.
- Use clear commit messages reflecting the feature or chunk implemented.

---

## Notes
- Prioritize core features first, then bonus/non-functional requirements as time allows.
- Adjust chunk sizes if you need to speed up or slow down.
- Track time spent per chunk if possible for future estimation.
