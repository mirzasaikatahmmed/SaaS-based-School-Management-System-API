# 18 — Grades Range & Generate Position (Marks)

Tenant-scoped grade scale and exam position generation from mark entries.

**Headers:** `Authorization: Bearer {token}` + `X-Tenant-ID: {slug}`  
**Updates:** `PATCH` (no PUT)

**Migration:** `AddGradesAttendanceLibraryEvents`  
**Provisioner:** `EnsureGradesAttendanceLibraryEventsModuleAsync` (seeds A+…F)

## Grades — `/api/marks/grades`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | List by sort order |
| POST | `/` | Create; unique name; no overlapping % ranges |
| PATCH | `/{id}` | Update |
| DELETE | `/{id}` | Blocked if exam positions exist |

**Seed:** A+(80–100, 5.00), A(70–79, 4.00), A-(60–69, 3.50), B(50–59, 3.00), C(40–49, 2.00), D(33–39, 1.00), F(0–32, 0.00)

## Positions — `/api/marks/positions`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/?examId=&classId=&sectionId=&academicYear=` | Load rows (`totalMarks` as `558/750`) |
| POST | `/generate` | Sum mark entries → full marks from schedule → % → GPA → PASS/FAIL → rank |
| POST | `/save` | Batch save position + principal/teacher comments |

## Auth

| Role | Access |
|------|--------|
| Super Admin / Admin | Full |
| Teacher | View + generate + save positions/comments |
