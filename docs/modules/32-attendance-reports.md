# 32 — Attendance Reports

Portal-style **Reports → Attendance Reports** screens.

**Headers:** Bearer + `X-Tenant-ID`  
**Base path:** `/api/reports/attendance`  
**Subject capture (optional):** `/api/attendance/subject`

Weekends default to **Friday + Saturday** (`school_settings.weekend_days = "5,6"`; Sunday=0 … Saturday=6).  
Holidays come from `events` where `is_holiday = true`.

Monthly grids use codes: **W** weekend, **H** holiday, **P** present, **A** absent, **L** late, **HD** half day.  
Summary `%` = (P + L + HD) / working days × 100 (working = days that are not W/H).

---

## Endpoints

| Method | Path | Permission | Portal screen |
|--------|------|------------|---------------|
| GET | `/students/monthly?classId=&sectionId=&year=&month=` | `Reports.AttendanceStudent` View | Student Reports |
| GET | `/students/daily?date=` | `Reports.AttendanceStudentDaily` View | Student Daily Reports |
| GET | `/students/overview?classId=&sectionId=&attendanceType=&fromDate=&toDate=&export=csv` | `Reports.AttendanceStudentOverview` View | Student Overview Reports |
| GET | `/subject-wise?classId=&sectionId=&subjectId=&date=` | `Reports.AttendanceSubjectWise` View | Subject Wise Reports |
| GET | `/subject-wise/by-day?classId=&sectionId=&date=` | `Reports.AttendanceSubjectWiseByDay` View | Subject Wise By Day |
| GET | `/subject-wise/by-month?classId=&sectionId=&subjectId=&year=&month=` | `Reports.AttendanceSubjectWiseByMonth` View | Subject Wise By Month |
| GET | `/employees/monthly?role=&year=&month=` | `Reports.AttendanceEmployee` View | Employee Reports |
| GET | `/exams?examId=&classId=&sectionId=&subjectId=&export=csv` | `Reports.AttendanceExam` View | Exam Reports |
| GET | `/fingerprint?role=&classId=&sectionId=&from=&to=&deviceId=&kind=&search=&page=&pageSize=&export=csv` | `Reports.AttendanceFingerprint` View | Attendance Report (fingerprint logs) |

### Fingerprint / biometric punch logs (Attendance Report List)

Portal filters: **Role** (Student / Teacher / …), **Class**, **Section**, **Date** range.  
Every device punch is a separate row — **not** collapsed by day.

| Column | Field |
|--------|-------|
| Photo | `photoUrl` |
| Name | `name` |
| Roll | `roll` (students) |
| Admission No | `admissionNo` (register no / staff id) |
| Register ID | `registerId` (device PIN) |
| Punch Time | `punchTime` / `punchTimeIso` / `punchDate`+`punchClock` |
| Terminal ID | `terminalId` (device serial) |

Also: `receivedAt` when the API stored the log. `from`/`to` inclusive; midnight `to` expands to end-of-day.

Kinds: `StudentDaily`, `EmployeeDaily`, `Exam`, `Unmapped`.

### Subject attendance capture

Required for subject-wise reports (day-wise class attendance stays on `/api/attendance/student`).

| Method | Path | Permission |
|--------|------|------------|
| GET | `/api/attendance/subject?classId=&sectionId=&subjectId=&date=` | `Attendance.StudentAttendance` View |
| PATCH | `/api/attendance/subject/save` | `Attendance.StudentAttendance` Edit |

```json
{
  "classId": "…",
  "sectionId": "…",
  "subjectId": "…",
  "attendanceDate": "2026-08-10",
  "items": [{ "studentId": "…", "status": "Present", "remarks": null }]
}
```

Statuses: `Present`, `Absent`, `Late`, `HalfDay`.

---

## Response shapes (summary)

### Monthly grid (`students/monthly`, `subject-wise/by-month`, `employees/monthly`)

```json
{
  "title": "…",
  "year": 2026,
  "month": 8,
  "legend": [{ "code": "W", "label": "Weekends" }],
  "dayColumns": [{ "day": 1, "key": "01", "dayName": "Sat", "isWeekend": true, "isHoliday": false }],
  "rows": [{
    "id": "…",
    "name": "…",
    "days": [{ "day": 1, "key": "01", "code": "W" }],
    "percentage": 0,
    "weekendCount": 9,
    "presentCount": 0,
    "absentCount": 0,
    "lateCount": 0,
    "halfDayCount": 0
  }]
}
```

### Student daily

Per class–section: present / absent counts and percentages for one date.

### Student overview

Students in class/section with `count` of days matching `attendanceType` in the date range.

### Exam

List: `#`, name, register no, roll, subject, remarks, status (empty status if not marked).

### Fingerprint logs

Portal **Attendance Report List**: Role + Class/Section + date range.  
One row per punch: photo, name, roll, admission no, register id (PIN), punch time, terminal id. `export=csv` supported.

---

## Schema

Migration: `20260809205248_AddAttendanceReports`

- `student_subject_attendance` — unique `(student_id, subject_id, attendance_date)`
- `school_settings.weekend_days` — default `'5,6'`

Also applied via `EnsureSettingsModuleAsync` for existing tenants.
