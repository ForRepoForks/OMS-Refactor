# Refactoring Checklist – Controllers (2025-04-22)

> **Note:** All refactoring must follow strict Test-Driven Development (TDD):
> - Write or update automated tests before changing production code
> - Ensure tests fail (or are inconclusive) before implementation
> - Only write the minimum code needed to pass the test
> - Refactor only after all tests are green
>
> **Code Style:** Always use automatic code formatting tools (e.g., `dotnet format`, IDE format-on-save) to maintain code style consistency.

This checklist tracks actionable refactoring tasks for the API controllers and related code in the Order Management System. For this 2–3 hour session, the focus is on the ProductsController and related logic only. All other items are moved to a 'Future Refactoring' section.

> **Note:** The OrdersController refactor introduces a greater number of dedicated DTOs (request/response types) compared to the ProductsController refactor. This is due to the increased complexity and variety of data flows in order management (e.g., order creation, invoice, reporting), whereas the ProductsController required only minimal DTOs for its simpler contract.

---

## 🚩 Refactoring Focus: All Controllers (2–3 hour session)

### Goal
- Address the most critical maintainability, testability, and correctness issues in all controllers (ProductsController, OrdersController) within 2–3 hours. Prioritize high-impact, cross-cutting improvements over exhaustive detail.

### Actionable Tasks
- [ ] Write or update tests for critical controller logic (TDD-first)
- [ ] Move obvious business logic from controllers into service classes (e.g., ProductService, OrderService)
- [ ] Move direct data access out of controllers and into services
- [ ] Move key DTOs out of controllers and into dedicated files
- [ ] Standardize validation and error handling (use data annotations/validators, unified error responses)
- [ ] Add basic input sanitization for user-facing endpoints (e.g., product search, order creation)

---

## ⏳ Future Refactoring (after this session)

- Refactor OrdersController and reporting logic
- Move all order-related DTOs to dedicated files
- Centralize and standardize validation & error handling for all endpoints
- Refactor discounted product report for query efficiency
- Implement robust test isolation for the entire test suite
- Add API versioning to all routes
- Ensure all endpoints have clear Swagger/OpenAPI documentation and XML summaries
- Review navigation properties and constructors in all models
- Ensure EF Core migrations are always up-to-date and automate migration application on startup
- Add or enforce code formatting configuration (e.g., `.editorconfig`, `dotnet format`)

---

**Tip:** Commit after each logical chunk. Regularly run tests and update documentation as needed.
