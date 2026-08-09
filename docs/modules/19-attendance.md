# 19 — Attendance (Student / Employee / Exam)

Daily student & employee attendance plus exam-subject attendance.

**Headers:** `Authorization: Bearer {token}` + `X-Tenant-ID: {slug}`  
**Updates:** batch save via `POST` (upsert)

**Provisioner:** `EnsureGradesAttendanceLibraryEventsModuleAsync`

## Student — `/api/attendance/student`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/?classId=&sectionId=&date=` | Active students + existing status |
| POST | `/save` | Batch upsert (`Present\|Absent\|Late\|Half Day`) |
| GET | `/report` | Date-range report |

## Employee — `/api/attendance/employee`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/?role=&date=` | Employees by role + photo |
| POST | `/save` | Batch upsert |
| GET | `/report` | Date-range report |

## Exam — `/api/attendance/exam`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/?examId=&classId=&sectionId=&subjectId=` | Class students |
| POST | `/save` | Batch upsert (`Present\|Absent\|Late` — no Half Day) |

## Auth

| Role | Access |
|------|--------|
| Super Admin / Admin | Full all types |
| Teacher | Student attendance (own class) |
| Student / Employee | Own report only |
