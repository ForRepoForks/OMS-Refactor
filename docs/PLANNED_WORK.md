# Planned Work and TODOs

This document tracks planned improvements and technical debt for the OMS-Refactor project.

---

## 1. Normalize Line Endings for GitHub Compatibility
- **Task:** Normalize all text file line endings in the repository to LF (Unix-style) for GitHub compatibility.
- **Steps:**
  1. Add or update a `.gitattributes` file with: `* text=auto eol=lf`
  2. Convert all text files (including README.md) to LF line endings.
  3. Use `git add --renormalize .` and commit the changes.
- **Rationale:** Ensures cross-platform consistency and prevents unnecessary diffs due to line ending mismatches.

---

## 2. Enforce Unique Product Names and Implement Duplicate Name Validation/Test
- **Task:** Enforce unique product names in the database and in ProductService.
- **Steps:**
  1. Add a unique index on the `Product.Name` column via EF Core migration.
  2. Update `ProductService.CreateProductAsync` to check for existing products with the same name and throw a `ValidationException` if found.
  3. Enable and implement the test `CreateProduct_DuplicateName_ThrowsValidationException` in ProductServiceTests (this test currently exists as a commented-out placeholder in the test file; uncomment and finalize it once the unique constraint and validation logic are in place).
  4. Use a real `OrderManagementContext` in the test to persist products and verify uniqueness.
  5. Apply the migration to update the schema.
- **Note:** The duplicate name test is already present (commented out) in `ProductServiceTests.cs` and should be enabled as soon as the above steps are complete.
- **Rationale:** Ensures product names are unique and prevents duplicate entries, improving data integrity.

---

## 3. Remove Nulls from Codebase
- **Task:** Eliminate the use of `null` in logic, tests, and data models wherever possible.
- **Steps:**
  1. Refactor all tests to avoid passing or expecting `null` values, especially for dependencies and data contexts (e.g., always use a real or in-memory context instead of `null`).
  2. Update data models and DTOs to use non-nullable reference types and provide sensible defaults.
  3. Refactor service and controller logic to leverage nullable reference type annotations and avoid returning or accepting `null` where not strictly required.
  4. Add nullability annotations and enable nullable reference types in all projects (`#nullable enable`).
  5. Address all compiler warnings related to nullability, ensuring code is safe and expressive.
  6. Add tests to verify that APIs and services do not accept or emit `null` values inappropriately.
- **Rationale:** Prevents null reference exceptions, improves code safety, and ensures codebase robustness and clarity.
