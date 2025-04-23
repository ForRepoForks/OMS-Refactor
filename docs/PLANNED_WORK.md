# Planned Work and TODOs

This document tracks planned improvements and technical debt for the OMS-Refactor project.

---

## 1. Normalize Line Endings for GitHub Compatibility

**Task:** Normalize all text file line endings in the repository to LF (Unix-style) for GitHub compatibility.

**Steps:**
1. Add or update a `.gitattributes` file with: `* text=auto eol=lf`
2. Convert all text files (including README.md) to LF line endings.
3. Use `git add --renormalize .` and commit the changes.

**Rationale:** Ensures cross-platform consistency and prevents unnecessary diffs due to line ending mismatches.

---

## 2. Enforce Unique Product Names and Implement Duplicate Name Validation/Test

**Status: Completed**

**What was done:**
- Added a unique index on the `Product.Name` column via EF Core migration.
- Updated `ProductService.CreateProductAsync` to check for existing products with the same name and throw a `ValidationException` if found.
- Enabled and validated the test for duplicate names in both integration and service tests.
- Applied the migration to update the schema.

**Rationale:** Ensures product names are unique and prevents duplicate entries, improving data integrity.

---

## 3. Review and Improve Test Assertions

**Progress:**
- Expanded edge case and error case coverage in `ProductApiTests` (invalid input, duplicate names, invalid pagination, non-existent entities, etc.).
- Assertions comprehensively cover HTTP status codes, DTO mapping, business logic, and collection contents.
- Negative tests and edge cases are present and maintained.

**Suggestions for Further Improvement:**
1. Add assertions for empty result sets, maximum/minimum field values, and error messages in failed responses.
2. Validate the full response schema for critical endpoints.
3. Expand negative testing (e.g., unauthorized access, invalid routes, malformed requests).
4. Consider performance and paging behavior for large datasets.

**Rationale:** Ensures the test suite not only validates correctness but also covers edge cases, error handling, and API contract robustness.

---

## 4. Remove Nulls from Codebase

**Task:** Eliminate the use of `null` in logic, tests, and data models wherever possible.

**Steps:**
1. Refactor all tests to avoid passing or expecting `null` values.
2. Update data models and DTOs to use non-nullable reference types and provide sensible defaults.
3. Refactor service and controller logic to leverage nullable reference type annotations.
4. Add nullability annotations and enable nullable reference types in all projects.
5. Address all compiler warnings related to nullability.
6. Add tests to verify that APIs and services do not accept or emit `null` values inappropriately.

**Rationale:** Prevents null reference exceptions, improves code safety, and ensures codebase robustness and clarity.
