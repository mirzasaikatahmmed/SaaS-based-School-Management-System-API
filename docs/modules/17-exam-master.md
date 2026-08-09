# 17 — Exam Master

Tenant-scoped exam setup: terms, halls, mark distributions, exams, schedules, and mark entries.

**Headers:** `Authorization: Bearer {token}` + `X-Tenant-ID: {slug}`

**Migration:** `20260809080441_AddExamMasterModule`  
**Provisioner:** `EnsureExamMasterModuleAsync` (chains Academic first; seeds terms + WRITTEN/MCQ)

## Seed data

| Master | Defaults |
|--------|----------|
| Exam terms | Mid Term, Half Yearly, Annual Exam, Pre-test |
| Mark distributions | WRITTEN, MCQ |
| Halls | none (admin creates) |

## API

### Exam Terms — `/api/exam/terms`
| Method | Path | Roles |
|--------|------|-------|
| GET | `/` | Admin, SuperAdmin, Teacher |
| POST | `/` | Admin, SuperAdmin |
| PUT | `/{id}` | Admin, SuperAdmin |
| DELETE | `/{id}` | Admin, SuperAdmin — blocked if used by exams |

### Exam Halls — `/api/exam/halls`
| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | List |
| GET | `/lookup` | `{ id, hallNo, seats }` |
| POST / PUT / DELETE | | Unique `hallNo`; delete blocked if used in schedule subjects |

### Mark Distributions — `/api/exam/mark-distributions`
CRUD; unique name; delete blocked if linked to exams.

### Exams — `/api/exam/exams`
| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | List (students see published only) |
| GET | `/lookup` | `{ id, name, termName }` |
| GET | `/{id}` | Detail + mark distribution names |
| POST / PUT | | Multi-select `markDistributionIds` → junction |
| PUT | `/{id}/publish` | Toggle `isPublished` |
| PUT | `/{id}/publish-result` | Toggle `isResultPublished` |
| DELETE | `/{id}` | Blocked if schedules or mark entries exist |

### Schedules — `/api/exam/schedules`
| Method | Path | Notes |
|--------|------|-------|
| GET | `/?classId=&sectionId=` | Filter list |
| GET | `/{id}` | Modal detail (formatted dates/times) |
| POST | `/` | Unique exam+class+section; subjects auto from class assignments if empty + quick-apply params |
| PUT | `/{id}` | Replace subject rows |
| DELETE | `/{id}` | Cascade subjects |

### Mark Entries — `/api/exam/mark-entries`
| Method | Path | Notes |
|--------|------|-------|
| GET | `/?examId=&classId=&sectionId=&subjectId=` | Active students + existing marks; `hasMcq` when exam has MCQ |
| POST | `/save` | Batch upsert; `totalMark = written + mcq`; absent → null marks |
| GET | `/export` | CSV (`export=csv` or `excel`) |

Written mark must be `0 ≤ mark ≤ writtenFullMark` from the schedule subject row.

## Roles

| Role | Access |
|------|--------|
| Super Admin / School Admin | Full |
| Teacher | Read schedule; submit mark entries |
| Student | Published schedules; own marks when result published |

## Tables

`exam_terms`, `exam_halls`, `mark_distributions`, `exams`, `exam_mark_distributions`, `exam_schedules`, `exam_schedule_subjects`, `mark_entries`
