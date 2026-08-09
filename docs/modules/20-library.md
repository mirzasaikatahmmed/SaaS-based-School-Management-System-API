# 20 — Library

Book categories, catalog (MinIO covers), issue/return with fine & stock tracking.

**Headers:** `Authorization: Bearer {token}` + `X-Tenant-ID: {slug}`  
**Updates:** `PATCH` (no PUT)  
**Fine:** `AppConstants.LibraryFinePerDay` (default **2** BDT/day overdue)

## Categories — `/api/library/categories`

CRUD; cannot delete if books reference the category.

## Books — `/api/library/books`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | List (`availableCopies = totalStock - issuedCopies`) |
| GET | `/lookup` | Dropdown |
| GET | `/{id}` | Detail |
| POST | `/` | Create |
| PATCH | `/{id}` | Update |
| DELETE | `/{id}` | Blocked if copies issued |
| POST | `/{id}/cover` | Cover → MinIO `library/covers/...` |

## Issues — `/api/library/issues`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | Admin list (auto-marks Overdue) |
| POST | `/` | Issue; 400 if no available copies; increments `issuedCopies` |
| PATCH | `/{id}/return` | Return + fine; decrement stock counter |
| GET | `/my` | Current user issued books |
| GET | `/lookup/borrowers?role=` | Students or employees by role |

## Auth

| Role | Access |
|------|--------|
| Super Admin / Admin / Librarian | Full |
| Student / Employee / Teacher | `GET /my` only |
