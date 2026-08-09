# 36 — Public Website (Frontend endpoints)

Anonymous APIs for the school public site (ahskbera.edu.bd–style pages).

**Status:** Public read endpoints implemented (`PublicWebsiteController`). Admin CMS CRUD can be added later; seed/edit content via DB or forthcoming `/api/website/*` admin routes.

**Controller:** `PublicWebsiteController` · **Route:** `/api/public` · **Auth:** `AllowAnonymous` + **required** `X-Tenant-ID`  
**Isolation:** One PostgreSQL schema + MinIO bucket per school — CMS/results/students never cross tenants.

**Headers:**

```http
X-Tenant-ID: {school-slug}
```

**Provisioner:** `EnsureWebsiteModuleAsync` (also run from `ProvisionAsync` on school create) · **Migrations:** `AddPublicWebsite`, `AddPublicAcademic`, `AddPublicStudentsResults`

Response wrapper: `ApiResponse<T>`.

---

## Page → endpoint map (from screenshots)

| Page | Route(s) |
|------|----------|
| Home (slider, speeches preview, notices, gallery, links, visitors) | `GET /api/public/home` |
| History (ইতিহাস) | `GET /api/public/about/history` |
| President speech (সভাপতির বাণী) | `GET /api/public/about/speeches/president` |
| Headmaster speech (প্রধান শিক্ষকের বাণী) | `GET /api/public/about/speeches/headmaster` |
| Presidents list (সভাপতিগণের নামের তালিকা) | `GET /api/public/leadership/presidents` |
| Headmasters list (প্রধান শিক্ষকগণের নামের তালিকা) | `GET /api/public/leadership/headmasters` |
| Managing committee (ম্যানেজিং কমিটি) | `GET /api/public/leadership/committee` |
| Teachers list (শিক্ষক মণ্ডলী) | `GET /api/public/staff/teachers` |
| Office staff (অফিস স্টাফ) | `GET /api/public/staff/office` |
| Documents / papers (পাঠদান, স্বীকৃতি, শাখা, এমপিও…) | `GET /api/public/documents` |
| Class routine | `GET /api/public/academic/routines/class-routine` |
| School exam routine | `GET /api/public/academic/routines/school-exam-routine` |
| SSC exam routine | `GET /api/public/academic/routines/ssc-exam-routine` |
| SSC vocational exam routine | `GET /api/public/academic/routines/ssc-vocational-exam-routine` |
| Prospectus / admission / lesson / library / lab | `GET /api/public/academic/pages/{slug}` |
| Handnote / content | `GET /api/public/academic/handnotes` |
| Online class | `GET /api/public/academic/online-classes?className=` |
| Layout (every page) | `GET /api/public/site/settings`, `/menu`, `/footer` |
| Notices full list | `GET /api/public/notices` |
| Photo gallery | `GET /api/public/gallery` · `GET /api/public/gallery/{albumId}` |
| Golden Jubilee | `GET /api/public/academic/pages/golden-jubilee` |
| SSC exam results | `GET /api/public/results/ssc` |
| Result analytics | `GET /api/public/results/analytics` |
| Student statistics | `GET /api/public/students/statistics` |
| Student list (by class/section) | `GET /api/public/students?className=&sectionName=` |
| Contact | `GET /api/public/contact` · `POST /api/public/contact/messages` |

**Reuse existing (already live):**

| Feature | Path |
|---------|------|
| Online admission | `POST /api/online-admission/apply`, `GET …/track/{ref}`, `GET …/lookup/classes/{slug}` |
| Public events | `GET /api/events/public` |
| Portal login | `POST /api/auth/login` |

---

## Site layout — `/api/public/site`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/settings` | School name, logo, phone, email, address, social URLs, copyright, portal URL |
| GET | `/menu` | Header nav tree (Home, About, Academic, …) |
| GET | `/footer` | Footer columns (Institution / Student / Academic / Other links) |
| GET | `/visitors` | Views today, last 7 days, total, server time |
| POST | `/visitors/hit` | Increment visitor counter; returns updated stats |

### `settings` example `data`

```json
{
  "schoolName": "Adarsha High School, Kaitola",
  "schoolNameBn": "আদর্শ উচ্চ বিদ্যালয়, কৈটোলা",
  "phone": "+8801309125316",
  "email": "ahskbera@gmail.com",
  "address": "Kaitola, Bera, Pabna",
  "logoUrl": "https://…",
  "facebookUrl": "https://facebook.com/ahskberaofficial",
  "youtubeUrl": "https://…",
  "portalUrl": "/portal",
  "copyrightText": "© 2024 Adarsha High School, Kaitola"
}
```

---

## Home — `/api/public/home`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/public/home` | Single payload for homepage widgets |

### `data` shape

```json
{
  "slider": [{ "imageUrl": "…", "caption": "…", "buttonText": null, "buttonUrl": null }],
  "presidentPreview": {
    "name": "Nazma Shahin",
    "nameBn": "জনাব নাজমা শাহীন",
    "designation": "President",
    "designationBn": "সভাপতি",
    "photoUrl": "…",
    "messageHtml": "<p>…excerpt…</p>",
    "readMorePath": "/about/speeches/president"
  },
  "headmasterPreview": {
    "name": "Mirza Kamal Pasha",
    "nameBn": "মির্জা কামাল পাশা",
    "designation": "Headmaster (Acting)",
    "designationBn": "প্রধান শিক্ষক (ভারপ্রাপ্ত)",
    "photoUrl": "…",
    "messageHtml": "<p>…excerpt…</p>",
    "readMorePath": "/about/speeches/headmaster"
  },
  "notices": [
    { "id": "…", "sl": 1, "publishedOn": "2024-07-15", "subject": "…", "viewUrl": "…", "fileUrl": null }
  ],
  "galleryPreview": [
    { "id": "…", "title": "…", "thumbUrl": "…", "imageUrl": "…", "categoryName": "…" }
  ],
  "importantLinks": [
    { "id": "…", "label": "Ministry of Education", "url": "https://…", "sortOrder": 1 }
  ],
  "visitorStats": {
    "viewsToday": 12,
    "viewsLast7Days": 80,
    "totalViews": 12000,
    "serverTime": "2026-08-10T03:18:00Z"
  },
  "facebookPageUrl": "https://facebook.com/ahskberaofficial",
  "onlineAdmissionEnabled": true
}
```

---

## About — `/api/public/about`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/history` | History page: profile sidebar + HTML sections + optional founding committee table |
| GET | `/speeches/president` | Full president message |
| GET | `/speeches/headmaster` | Full headmaster message |

### Speech `data`

```json
{
  "title": "President's Message",
  "titleBn": "সভাপতির বাণী",
  "name": "Nazma Shahin",
  "nameBn": "জনাব নাজমা শাহীন",
  "designation": "President",
  "designationBn": "সভাপতি",
  "photoUrl": "…",
  "messageHtml": "<p>…</p>",
  "phone": null,
  "email": null,
  "facebookUrl": null
}
```

### History `data`

```json
{
  "title": "History",
  "titleBn": "ইতিহাস",
  "profile": {
    "eiin": "125341",
    "establishedYear": 1972,
    "schoolType": "Secondary High School",
    "classesOffered": "6th to 10th",
    "totalStudentsLabel": "300+",
    "website": "www.ahskbera.edu.bd",
    "address": "Village: Kaitola, Upazila: Bera, District: Pabna",
    "imageUrl": "…"
  },
  "sections": [
    { "heading": "Brief Description", "headingBn": "সংক্ষিপ্ত বর্ণনা", "bodyHtml": "<p>…</p>" },
    { "heading": "History", "headingBn": "ইতিহাস", "bodyHtml": "<p>…</p>" }
  ],
  "foundingCommittee": [
    { "sl": 1, "name": "…", "designation": "President" }
  ]
}
```

---

## Leadership — `/api/public/leadership`

| Method | Path | Query | Description |
|--------|------|-------|-------------|
| GET | `/presidents` | `?search=` | Tenure list: name, joinedOn, leftOn |
| GET | `/headmasters` | `?search=` | Tenure list: name, designation, joinedOn, leftOn |
| GET | `/committee` | — | Managing committee grouped by category |

### Tenure row

```json
{
  "id": "…",
  "sl": 1,
  "name": "মির্জা আব্দুল আউয়াল",
  "designation": null,
  "joinedOn": "1972-12-31",
  "leftOn": "1978-06-01"
}
```

### Committee `data`

```json
{
  "categories": [
    {
      "key": "President",
      "title": "President",
      "titleBn": "সদস্য পদের শ্রেণিঃ সভাপতি",
      "members": [
        {
          "id": "…",
          "sl": 1,
          "name": "…",
          "designation": "সভাপতি",
          "category": "President",
          "photoUrl": "…",
          "mobileNo": "+880…"
        }
      ]
    },
    { "key": "GuardianRepresentative", "title": "…", "titleBn": "…অভিভাবক (সাধারণ)", "members": [] },
    { "key": "TeacherRepresentative", "title": "…", "titleBn": "…শিক্ষক প্রতিনিধি", "members": [] },
    { "key": "MemberSecretary", "title": "…", "titleBn": "…সদস্য সচিব", "members": [] }
  ]
}
```

---

## Staff directories — `/api/public/staff`

Public read-only lists (same columns as live teacher / office-staff tables).

| Method | Path | Query | Description |
|--------|------|-------|-------------|
| GET | `/teachers` | `?search=` | Teaching staff directory |
| GET | `/office` | `?search=` | Office / support staff |

### Staff row

```json
{
  "id": "…",
  "sl": 1,
  "name": "Mirza Kamal Pasha",
  "indexNo": "R568131",
  "designation": "Headmaster (Acting)",
  "subject": "Physics",
  "photoUrl": "…",
  "mobileNos": ["+8801729987720"],
  "email": "mirzakamalpasa@gmail.com",
  "qualifications": [
    { "degree": "M.Sc", "result": "…", "year": 2000 }
  ],
  "firstJoiningDate": "2003-02-01",
  "mpoDate": null,
  "presentJoiningDate": "2003-02-01",
  "dateOfBirth": "1977-10-09"
}
```

Notes:

- Teachers ≈ employees with role `teacher` (plus headmaster designation) exposed for website.
- Office staff ≈ employees with role `staff` (or a website visibility flag when CMS ships).
- Sensitive bank fields are **never** returned on public routes.

---

## Notices — `/api/public/notices`

| Method | Path | Query | Description |
|--------|------|-------|-------------|
| GET | `/` | `?limit=&search=` | Notice board list |
| GET | `/{id}` | — | Detail (+ optional `bodyHtml`) |

```json
{
  "id": "…",
  "sl": 1,
  "publishedOn": "2024-07-15",
  "subject": "Half-yearly exam routine",
  "viewUrl": "/notices/…",
  "fileUrl": "https://…/routine.pdf",
  "bodyHtml": null
}
```

---

## Gallery — `/api/public/gallery`

| Method | Path | Query | Description |
|--------|------|-------|-------------|
| GET | `/categories` | — | Category list |
| GET | `/` | `?categoryId=&limit=` | Album / image list |
| GET | `/{id}` | — | Album detail + image URLs |

---

## Documents — `/api/public/documents`

For pages that embed PDFs (routines, MPO / recognition / branch papers).

| Method | Path | Query | Description |
|--------|------|-------|-------------|
| GET | `/` | `?category=&search=` | Document list |
| GET | `/{id}` | — | Single document (`fileUrl` for viewer) |

`category` examples: `teaching`, `recognition`, `branch`, `mpo`, `routine`, `class-routine`, `ssc-exam-routine`, `other`.

```json
{
  "id": "…",
  "title": "Half-Yearly Examination Schedule - 2023",
  "titleBn": "অর্ধ বার্ষিক পরীক্ষার সময় সূচি - ২০২৩",
  "category": "routine",
  "fileUrl": "https://…/schedule.pdf",
  "publishedOn": "2023-06-01"
}
```

---

## Academic — `/api/public/academic`

PDF/HTML pages (class routine, exam routines, prospectus, admission, library, lab, lesson plan), handnotes table, online-class YouTube grid.

| Method | Path | Query | Description |
|--------|------|-------|-------------|
| GET | `/pages/{slug}` | — | Content page + optional PDF (`fileUrl`) + related documents |
| GET | `/routines/{type}` | — | Convenience for routine slugs (validates type) |
| GET | `/handnotes` | `?className=&search=` | Handnote download table |
| GET | `/online-classes` | `?className=` | Videos grouped by class (embed URL included) |

**Page slugs:** `prospectus`, `admission-process`, `admission-test`, `admission-form`, `lesson-planning`, `library`, `laboratory`, `golden-jubilee`  
**Routine types:** `class-routine`, `school-exam-routine`, `ssc-exam-routine`, `ssc-vocational-exam-routine`

### Academic page `data`

```json
{
  "slug": "class-routine",
  "title": "Class Routine",
  "titleBn": "ক্লাস রুটিন",
  "bodyHtml": null,
  "fileUrl": "https://…/routine.pdf",
  "documents": []
}
```

### Handnote row

```json
{
  "id": "…",
  "sl": 1,
  "publishedOn": "2022-06-12",
  "className": "অষ্টম",
  "title": "বিজ্ঞান",
  "teacherName": "মির্জা কামাল পাশা",
  "downloadUrl": "https://…"
}
```

### Online class group

```json
{
  "className": "Class 8",
  "videos": [
    {
      "id": "…",
      "title": "Math 4.3",
      "youtubeUrl": "https://youtube.com/watch?v=…",
      "youtubeVideoId": "…",
      "embedUrl": "https://www.youtube.com/embed/…",
      "classDate": "2020-08-07"
    }
  ]
}
```

---

## Results — `/api/public/results`

CMS tables: `website_result_analytics`, `website_published_results`.

| Method | Path | Query | Description |
|--------|------|-------|-------------|
| GET | `/analytics` | — | SSC + SSC Vocational pass/fail and GPA distribution by year |
| GET | `/ssc` | `?examType=ssc\|vocational` | Published result links (“Enter” list) |

### Analytics `data`

```json
{
  "sscExam": {
    "examType": "ssc",
    "title": "SSC Exam",
    "passFailStats": [
      { "year": 2024, "appeared": 120, "passed": 115, "notPassed": 5, "passPercent": 95.83, "gpa5": 12, "gpa5Percent": 10.0 }
    ],
    "gpaDistribution": [
      { "year": 2024, "gpa5": 12, "gpa4x": 40, "gpa3x": 35, "gpa2x": 20, "gpa1x": 8 }
    ]
  },
  "sscVocational": { "examType": "ssc-vocational", "title": "SSC Vocational Exam", "passFailStats": [], "gpaDistribution": [] }
}
```

### Published result row

```json
{
  "id": "…",
  "title": "SSC Examination Result 2024",
  "titleBn": "এসএসসি পরীক্ষার ফলাফল ২০২৪",
  "examType": "ssc",
  "year": 2024,
  "detailUrl": "/results/ssc/2024",
  "fileUrl": null
}
```

---

## Students (public) — `/api/public/students`

Active students only. Public fields: photo, name, class, section, register no, roll, father/mother. No mobile/email.

| Method | Path | Query | Description |
|--------|------|-------|-------------|
| GET | `/statistics` | `?academicYear=` | Class × section male/female counts |
| GET | `/` | `?classId=` or `className=` · `sectionId=` / `sectionName=` · `search=` · `page=` · `pageSize=` · `academicYear=` | Paginated public roster |

`className` accepts labels like `6`, `Class 6`, `six`, `IX`, etc.

---

## Contact — `/api/public/contact`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/` | Address, phones, email, map iframe, form labels |
| POST | `/messages` | Submit contact form |

### POST body

```json
{
  "name": "…",
  "email": "…",
  "phone": "…",
  "subject": "…",
  "message": "…"
}
```

---

## Auth / tenancy rules

**Each school has fully separate data.** Public website CMS, students, results, gallery, contact messages, and files live only in that school’s PostgreSQL schema (`tenant_{slug}`) and MinIO bucket (`school-{slug}`). There is no shared website table across tenants.

1. Resolve tenant from `X-Tenant-ID` (school slug). Missing → `400`. Unknown slug → `404`. Inactive → `403`.
2. `PublicWebsiteController` requires a resolved tenant (`[RequireTenant]`); all queries use that schema only.
3. New schools get empty website tables at provision time (`EnsureWebsiteModuleAsync`); first public hit also ensures tables exist.
4. Only return content flagged for website (`showWebsite` / `isPublished` where applicable).
5. No admin/payroll/student PII beyond intentionally public staff/student directory fields.
6. Updates from admin panel will use authenticated routes under `/api/website/…` (CMS) when shipped; public GETs remain stable.
7. Empty CMS tables still return usable defaults for menu/footer; speeches/notices return empty or 404 as appropriate.
8. Teachers / office staff are read from that tenant’s `employees` only; bank fields are never exposed.
9. Media URLs are presigned against the tenant’s own bucket — never another school’s.

---

## Suggested frontend route aliases

| UI path | API |
|---------|-----|
| `/` | `GET /api/public/home` + site layout |
| `/history` | `GET /api/public/about/history` |
| `/president-speech` | `GET /api/public/about/speeches/president` |
| `/headmaster-speech` | `GET /api/public/about/speeches/headmaster` |
| `/presidents` | `GET /api/public/leadership/presidents` |
| `/headmasters` | `GET /api/public/leadership/headmasters` |
| `/administration` / `/committee` | `GET /api/public/leadership/committee` |
| `/teachers` | `GET /api/public/staff/teachers` |
| `/office-staff` | `GET /api/public/staff/office` |
| `/documents` | `GET /api/public/documents` |
| `/class-routine` | `GET /api/public/academic/routines/class-routine` |
| `/school-exam-routine` | `GET /api/public/academic/routines/school-exam-routine` |
| `/ssc-exam-routine` | `GET /api/public/academic/routines/ssc-exam-routine` |
| `/ssc-vocational-exam-routine` | `GET /api/public/academic/routines/ssc-vocational-exam-routine` |
| `/prospectus` | `GET /api/public/academic/pages/prospectus` |
| `/admission-process` | `GET /api/public/academic/pages/admission-process` |
| `/admission-test` | `GET /api/public/academic/pages/admission-test` |
| `/admission-form` | `GET /api/public/academic/pages/admission-form` |
| `/lesson-planning` | `GET /api/public/academic/pages/lesson-planning` |
| `/library` | `GET /api/public/academic/pages/library` |
| `/laboratory` | `GET /api/public/academic/pages/laboratory` |
| `/golden-jubilee` | `GET /api/public/academic/pages/golden-jubilee` |
| `/handnote-content` | `GET /api/public/academic/handnotes` |
| `/online-class` | `GET /api/public/academic/online-classes` |
| `/notices` | `GET /api/public/notices` |
| `/gallery` | `GET /api/public/gallery` |
| `/ssc-exam-results` | `GET /api/public/results/ssc` |
| `/result-analytics` | `GET /api/public/results/analytics` |
| `/student-statistics` | `GET /api/public/students/statistics` |
| `/student-list` | `GET /api/public/students?className=&sectionName=` |
| `/contact` | `GET /api/public/contact` |
