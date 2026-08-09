# 16 — Academic

**Controllers:** under `/api/academic/...`

Class & section control, class teachers, subjects, class subject assignment, class/teacher schedules, student promotion.

## Routes

| Area | Path |
|------|------|
| Classes | `/api/academic/classes` |
| Sections | `/api/academic/sections` |
| Class teachers | `/api/academic/class-teachers` |
| Subjects | `/api/academic/subjects` |
| Class–subject assign | `/api/academic/class-subject-assignments` |
| Class schedules | `/api/academic/class-schedules` |
| Teacher schedules | `/api/academic/teacher-schedules` |
| Promotion | `/api/academic/promotion` |

## Roles

| Role | Access |
|------|--------|
| Super Admin / School Admin | Full CRUD |
| Teacher | Read classes/sections/subjects/schedules; own teacher schedule |
| Student | Read own class schedule (`/class-schedules/my`) |

## Model notes

- Sections can be global masters (`class_id` nullable) with **Capacity**.
- Classes link sections via `class_sections` (M2M). Existing per-class sections are backfilled into the join.
- Class teacher: one per class+section (upsert).
- Subjects: unique **code**; types Theory / Practical / Mandatory / Optional.
- Schedule: one per class+section+day; periods support **IsBreak** (no subject/teacher).
- Promotion: updates student class/section/year/roll (or year-only / deactivate) and writes `student_promotions` history.

## Quick promotion statuses

`Promoted` · `Running` · `Left` · `Alumni`
