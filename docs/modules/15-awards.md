# 15 — Awards (Human Resource)

**Controller:** `AwardController` · **Route:** `/api/awards`

Give awards to employees or students. Tabs: Award List + Give Award.

## Endpoints

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/api/awards` | Admin+, Accountant | Paginated list |
| GET | `/api/awards/my` | Employee or Student | Own awards |
| POST | `/api/awards` | Admin+, Accountant | Give award |
| PUT | `/api/awards/{id}` | Admin+, Accountant | Update |
| DELETE | `/api/awards/{id}` | Admin+, Accountant | Delete |
| GET | `/api/awards/export` | Admin+, Accountant | csv / excel / pdf |
| GET | `/api/awards/lookup/winners?role=` | Admin+, Accountant | Winner dropdown |

## Recipient rules

| Role | Recipient field | Lookup source |
|------|-----------------|---------------|
| Student | `StudentId` | students → `"Name (RegisterNo)"` |
| Admin, Teacher, Accountant, Librarian, Receptionist, Staff, Demo | `EmployeeId` | employees by role → `"Name (StaffId)"` |

Exactly one of `EmployeeId` / `StudentId` must be set (DB check + validator).

## List columns

Sl, Branch, Winner, Role, Award Name, Gift Item, Cash Price, Award Reason, Given Date

## Notes

- Role is denormalized at save time.
- CashPrice optional (≥ 0 if set).
- GivenDate defaults to today (UTC date) when omitted.
- Same person may receive multiple awards.
