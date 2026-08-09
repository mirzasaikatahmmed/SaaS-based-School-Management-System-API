# 34 — Examination Reports (Report Card, Tabulation Sheet, Progress Reports)

Portal screens under **Reports → Examination**.

**Headers:** Bearer + `X-Tenant-ID`  
**Base path:** `/api/reports/examination`

Depends on mark entries, exam schedules (full marks), grade ranges, and (optional) generated exam positions + day-wise attendance.

---

## Student list (shared by Report Card / Progress)

| Method | Path | Permission |
|--------|------|------------|
| GET | `/students?classId=&sectionId=&academicYear=&examId=&search=` | `Reports.ExamReportCard` View |

Columns: sl, studentName, category, registerNo, roll, mobileNo.

---

## Report Card

Single exam → printable cards for selected students.

| Method | Path | Permission |
|--------|------|------------|
| POST | `/report-card` | `Reports.ExamReportCard` View |

```json
{
  "examId": "…",
  "classId": "…",
  "sectionId": "…",
  "academicYear": 2026,
  "studentIds": ["…"],
  "printAttendance": true,
  "printGradeScale": false,
  "printDate": "2026-08-10"
}
```

Each card includes: school header, student bio (father/mother/DOB/class), **main** subject rows, optional **additionalSubject** row, **continuousAssessment** rows, grand total + words, average %, `gpaWithoutAdditional` / `additionalGpAbove2` / `gpa` (SSC “GP above 2” formula), result, optional attendance summary, optional grade scale, print metadata.

Attendance summary (when `printAttendance`): working days (non-weekend / non-holiday from Jan 1 of academic year → print date), days attended (Present/Late/HalfDay), percentage.

---

## Progress Reports

Same card shape as report card, but **multi-exam** — returns one card per (exam × student).

| Method | Path | Permission |
|--------|------|------------|
| POST | `/progress` | `Reports.ExamProgress` View |

Body same as report card; use `examIds: ["…","…"]` (or single `examId`).

---

## Tabulation Sheet

Class matrix: students × subjects + total / GPA / result / position.

| Method | Path | Permission |
|--------|------|------------|
| GET | `/tabulation?examId=&classId=&sectionId=&academicYear=&export=csv` | `Reports.ExamTabulation` View |

Position shows stored exam-position rank, or `"Not Generated"` until positions are generated via `/api/exam/...` position APIs.

---

## Notes

- Subject full marks come from `exam_schedule_subjects.written_full_mark`.
- Grades from active `grade_ranges`.
- Prefer board-style GPA from subject GPs + declared Additional Subject (`max(0, GP−2)`); use stored `exam_positions` for result/position when present.
