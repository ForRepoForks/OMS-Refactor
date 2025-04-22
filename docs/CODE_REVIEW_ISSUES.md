# Code Review: Must-Fix Issues

## 1. Exception-Throwing Property Setter in Product
**File:** `OrderManagementSystem.API.Models.Product.cs`
- The `Price` property setter throws an `ArgumentException` if the value is zero or negative. This can cause runtime errors during model binding and EF Core operations. ASP.NET Core expects validation errors via data annotations, not exceptions.
- **How to Fix:** Remove the exception from the setter and use only data annotations or custom validation attributes.

## 2. DTOs Defined Inside Controllers
**Files:** `ProductsController.cs`, `OrdersController.cs`
- DTOs like `DiscountDto`, `CreateOrderRequest`, etc., are defined as nested classes inside controllers. This pollutes controller files and makes DTO reuse and documentation harder.
- **How to Fix:** Move all DTOs to a dedicated folder (e.g., `DTOs`) and reference them in controllers.

## 3. Inefficient Discounted Product Report Query
**File:** `OrdersController.cs`
- The `GetDiscountedProductReport` endpoint loops over each discounted product and runs a separate query for each, causing N+1 query problems. This won't scale for large datasets.
- **How to Fix:** Refactor to a single query using `GROUP BY`/`JOIN` to aggregate results in one go.

## 4. Inconsistent Error Messaging
**Files:** Controllers
- Some error responses are generic (`BadRequest(ModelState)`), others add custom error messages. This leads to inconsistent API responses.
- **How to Fix:** Standardize error responses, preferably using a common error format.

## 5. Exception Handling for ArgumentException
**File:** `ProductsController.cs`
- Only `ArgumentException` is caught in product creation. If other endpoints or EF Core trigger this, it may be unhandled.
- **How to Fix:** Use validation attributes for domain rules and ensure global exception handling middleware covers exceptions.

## 6. Test Cleanup Directly Modifies Database
**File:** `OrderApiTests.cs`
- `CleanupDatabaseAsync` deletes all records from tables. This can cause test flakiness if tests run in parallel.
- **How to Fix:** Use an in-memory database for tests, or run each test in a transaction and roll back.

## 7. Discount Removal Logic is Unintuitive
**File:** `ProductsController.ApplyDiscount`
- Setting both percentage and quantity to zero removes the discount, which is not intuitive for API consumers.
- **How to Fix:** Add a dedicated endpoint or a clear flag for removing discounts, or document this behavior clearly in Swagger.

## 8. No API Versioning
- No API versioning is implemented, which will make future breaking changes problematic for clients.
- **How to Fix:** Add versioning to API routes (e.g., `/api/v1/products`).

## 9. No Input Sanitization for Product Name Search
**File:** `ProductsController.GetProducts`
- The search uses `.ToLower().Contains(name.ToLower())`, which can be slow and is not culture-aware. No input length or character validation.
- **How to Fix:** Use EF Core's full-text search or index on `Name`. Validate/limit length of the search string.

## 10. Docker Compose: Migration Handling Not Evident
**File:** `docker-compose.yml`
- No clear step to run EF Core migrations on startup. Could cause runtime errors if DB schema is out of date.
- **How to Fix:** Add an entrypoint or command to apply migrations automatically.
