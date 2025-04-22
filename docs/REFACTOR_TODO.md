# Refactoring Checklist – Controllers (2025-04-22)

> **Note:** All refactoring must follow strict Test-Driven Development (TDD):
> - Write or update automated tests before changing production code
> - Ensure tests fail (or are inconclusive) before implementation
> - Only write the minimum code needed to pass the test
> - Refactor only after all tests are green
>
> **Code Style:** Always use automatic code formatting tools (e.g., `dotnet format`, IDE format-on-save) to maintain code style consistency.

This checklist tracks actionable refactoring tasks for the API controllers and related code in the Order Management System. Check off items as you complete them.

---

## ProductsController
- [ ] Extract business logic (e.g., discount validation, product creation) into a `ProductService`
- [ ] Move DTOs (e.g., `DiscountDto`) to dedicated files/folders
- [ ] Replace direct data access (`_context`) with service/repository abstraction
- [ ] Improve model validation (add granular error messages, consider custom validation attributes)
- [ ] Introduce request/response DTOs for clarity
- [ ] Standardize error responses (use a common error format)
- [ ] Remove exception-throwing property setters from models (use data annotations instead)
- [ ] Add/clarify API documentation (Swagger annotations, summaries)
- [ ] Add a dedicated endpoint or clear flag for removing discounts (avoid using zero values for this)
- [ ] Validate/sanitize input for product name search (length, characters)

## OrdersController
- [ ] Extract business logic (order creation, invoice calculation, discount application, report generation) into an `OrderService`
- [ ] Move nested DTO classes to separate files for maintainability
- [ ] Replace direct data access (`_context`) with service/repository abstraction
- [ ] Improve and extract validation logic for reuse and clarity
- [ ] Consider moving the report endpoint to a dedicated `ReportsController` for SRP
- [ ] Refactor discounted product report to avoid N+1 query problem (use a single query with GROUP BY/JOIN)
- [ ] Standardize error responses (use a common error format)
- [ ] Add/clarify API documentation (Swagger annotations, summaries)
- [ ] Add API versioning to routes (e.g., `/api/v1/orders`)

## Data & Models
- [ ] Remove exception-throwing property setters (e.g., `Product.Price`), use data annotations or custom validation attributes
- [ ] Review navigation properties and constructors for clarity and safety

## General Codebase
- [ ] Add/expand unit and integration tests for new service layers and validation logic
- [ ] Review and update API documentation (Swagger annotations, summaries)
- [ ] Ensure EF Core migrations are updated and reflect all model changes
- [ ] Standardize error responses across all endpoints
- [ ] Add input sanitization and validation for all user inputs
- [ ] Implement or document test isolation (unique DB per test, cleanup, or test containers)
- [ ] Ensure Docker Compose applies EF Core migrations on startup (avoid schema drift)

---

**Tip:** Commit after each logical chunk. Regularly run tests and update documentation as needed.
