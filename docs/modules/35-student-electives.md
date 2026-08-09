# 35 — Student Electives + Additional Subject (SSC GPA)

Matches Bangladesh board transcripts (e.g. Rajshahi Board SSC):

1. **Elective (class/exam):** student chooses **Higher Math XOR Agriculture** only.
2. **Biology is NOT an elective** — it is a normal subject everyone can take; mark it `canBeAdditional: true`.
3. **Additional Subject (GPA declaration):** student declares **either their elective OR Biology** as the board “Additional Subject”.
4. **GPA:** `(sum of main subject GPs + max(0, additionalGP − 2)) / count(main subjects)`.

---

## Setup (admin)

### 1. Create subjects

| Subject | Flags |
|---------|--------|
| Higher Math | normal; later marked elective on class assignment |
| Agriculture | normal; later marked elective on class assignment |
| Biology | `canBeAdditional: true` (not elective) |
| Physical Education / Career Education | `isContinuousAssessment: true` (graded, excluded from GPA) |

```json
POST /api/academic/subjects
{
  "name": "Biology",
  "code": "BIO",
  "canBeAdditional": true,
  "isContinuousAssessment": false
}
```

### 2. Class–subject assignment — only HM / Agri are elective

```json
POST /api/academic/class-subject-assignments
{
  "classId": "…",
  "sectionId": "…",
  "items": [
    { "subjectId": "…bangla…", "isElective": false },
    { "subjectId": "…biology…", "isElective": false },
    { "subjectId": "…higher-math…", "isElective": true, "electiveGroup": "4th" },
    { "subjectId": "…agriculture…", "isElective": true, "electiveGroup": "4th" }
  ]
}
```

Do **not** put Biology in the elective group.

### 3. Assign elective + declare Additional Subject

| Method | Path |
|--------|------|
| GET | `/api/academic/student-electives?classId=&sectionId=&academicYear=&electiveGroup=4th` |
| PATCH | `/api/academic/student-electives` |
| PATCH | `/api/academic/student-electives/bulk` |

Example — took Higher Math, declared Biology as Additional (like the SSC marksheet):

```json
{
  "studentId": "…",
  "subjectId": "…higher-math…",
  "additionalSubjectId": "…biology…",
  "classId": "…",
  "sectionId": "…",
  "academicYear": 2026,
  "electiveGroup": "4th"
}
```

- Omit `additionalSubjectId` → defaults to Biology if a `canBeAdditional` subject exists, else the elective.
- Valid additional: **your elective** OR any subject with `canBeAdditional` (Biology).

Bulk:

```json
{
  "classId": "…",
  "sectionId": "…",
  "academicYear": 2026,
  "electiveGroup": "4th",
  "choices": [
    { "studentId": "…", "subjectId": "…higher-math…", "additionalSubjectId": "…biology…" },
    { "studentId": "…", "subjectId": "…agriculture…", "additionalSubjectId": "…agriculture…" }
  ]
}
```

---

## GPA formula (board style)

Given main subjects (all graded subjects except continuous assessment and except the declared additional):

| Metric | Formula |
|--------|---------|
| GPA without additional | `sum(main GPs) / N` |
| GP Above 2 | `max(0, additionalGP − 2)` |
| GPA | `(sum(main GPs) + GP Above 2) / N` |

Example from SSC transcript: mains sum 34 over 9 subjects → 3.78; Biology GP 4 → above2 = 2 → GPA = 4.00.

Report cards expose:

- `subjects` — main rows  
- `additionalSubject` — declared additional row  
- `continuousAssessment` — PE / Career Education etc.  
- `gpaWithoutAdditional`, `additionalGpAbove2`, `gpa`

Exam position generation uses the same GPA logic.

---

## What this affects

| Area | Behaviour |
|------|-----------|
| Subject attendance / mark entry | Only enrolled students for Higher Math or Agriculture |
| Report card | Shows chosen elective; Biology always if scheduled; Additional + dual GPA |
| Tabulation | Non-chosen elective columns blank |

---

## Schema

- `subjects.can_be_additional`, `subjects.is_continuous_assessment`
- `class_subject_assignment_items.is_elective`, `elective_group`
- `student_subject_enrollments.subject_id` (elective), `additional_subject_id` (GPA declaration)

Migration / provisioner: `AddAdditionalSubjectGpa` (via `EnsureSettingsModuleAsync`).
