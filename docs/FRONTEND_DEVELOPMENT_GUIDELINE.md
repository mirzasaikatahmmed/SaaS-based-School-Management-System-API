# Frontend Development Guideline (Next.js)

**Audience:** Frontend engineers building the School Management UI against this API.  
**Stack (required):** **Next.js** (App Router) + TypeScript + React  
**API:** .NET 10 SaaS School Management System (`SchoolManagement.API`)  
**Module index:** [`MODULES.md`](./MODULES.md) · **Workflow:** [`SYSTEM_WORKFLOW.md`](./SYSTEM_WORKFLOW.md)

> This document is the single frontend build guide. It does **not** live in the repo root — keep it under `docs/`.  
> Do **not** invent a separate `frontend/` folder inside the API repo unless the team explicitly adds a monorepo app; the recommended layout is a **sibling Next.js app**.

---

## 1. Product surfaces (what to build)

Build **one Next.js app** (or a monorepo with shared packages) covering these surfaces:

| Surface | Who | Auth | Tenant header |
|---------|-----|------|----------------|
| **A. Platform admin** | Super Admin | JWT | Usually none (acts across schools) |
| **B. School portal** | Admin, Teacher, Accountant, Librarian, … | JWT | **Required** `X-Tenant-ID` |
| **C. Parent portal** | Parent / Guardian | JWT + tenant | Required |
| **D. Student portal** | Student | JWT + tenant | Required |
| **E. Public school website** | Anonymous visitors | None | **Required** `X-Tenant-ID` (or subdomain → slug) |
| **F. Public online admission** | Applicants | Anonymous | Tenant from slug / subdomain |

Suggested Next.js route groups:

```text
app/
  (public)/          → E + F  (marketing site + online admission)
  (auth)/login       → shared login
  (platform)/        → A      Super Admin
  (portal)/          → B      School staff dashboard
  (parent)/          → C
  (student)/         → D
```

Resolve school slug from:

1. Subdomain (`ahskbera.yoursaas.com` → `ahskbera`), or  
2. Path (`/s/ahskbera/...`), or  
3. Login form school selector (staff), or  
4. Env default for single-tenant demos.

Store slug in cookie / Zustand / React context and attach as `X-Tenant-ID` on every school-scoped request.

---

## 2. Recommended tech stack

| Layer | Choice |
|-------|--------|
| Framework | **Next.js 15+ (App Router)** |
| Language | TypeScript (strict) |
| UI | Tailwind CSS + your component library (shadcn/ui recommended) |
| Forms | React Hook Form + Zod (mirror FluentValidation rules from API errors) |
| Data fetching | TanStack Query (server state) + Next.js Server Components where auth allows |
| Tables | TanStack Table (sort / filter / pagination) |
| Charts | Recharts or Chart.js (result analytics, attendance) |
| HTTP | `fetch` wrapper or axios — single `apiClient` |
| Auth storage | httpOnly cookies preferred; or memory + refresh rotation |
| i18n | next-intl (EN + BN for public site / reports) |
| PDF / print | Browser print CSS or `@react-pdf/renderer` for report cards |
| Icons | lucide-react |

**Do not** call `/iclock/*` from the browser — that is for ZKTeco devices only.

---

## 3. Suggested Next.js project layout

Sibling to the API repo (preferred):

```text
SaaS-based-School-Management-System/
  SchoolManagement/                 ← API (this repo)
  school-mgmt-web/                  ← Next.js frontend (new)
    app/
    components/
    features/                       ← one folder per module (01–36)
    lib/
      api/                          ← typed clients per domain
      auth/
      tenant/
    hooks/
    types/
    messages/                       ← i18n
```

Feature-first modules (align with backend docs):

```text
features/
  auth/
  schools/
  students/
  parents/
  employees/
  payroll/
  academic/
  exams/
  attendance/
  library/
  events/
  accounting/
  messages/
  settings/
  biometric/
  reports/
  public-website/
```

---

## 4. API contract rules (must follow)

### 4.1 Base URL & env

```env
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
NEXT_PUBLIC_DEFAULT_TENANT_SLUG=   # optional demo only
```

### 4.2 Headers

```http
Authorization: Bearer {accessToken}
X-Tenant-ID: {school-slug}
Content-Type: application/json
```

| Caller | `Authorization` | `X-Tenant-ID` |
|--------|-----------------|---------------|
| Super Admin platform APIs | Yes | Usually omit |
| School portal / parent / student | Yes | **Always** |
| Public website `/api/public/*` | No | **Always** |
| Online admission public apply | No | Yes (slug) |
| Biometric `/iclock` | — | **Never from frontend** |

### 4.3 Response envelope

Every JSON API returns:

```ts
type ApiResponse<T> = {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[] | null;
  timestamp: string;
};
```

UI rules:

- If `success === false` → toast `message` + show `errors[]`
- Use `data` only when `success === true`
- Handle `401` → refresh token → retry once → else logout
- Handle `403` → “no permission” empty state
- Handle `404` tenant → “school not found”

### 4.4 HTTP verbs

- Create: `POST`
- Update: **`PATCH` only** (no `PUT`)
- Delete: `DELETE`
- Lists: `GET` with query filters + pagination

### 4.5 Auth flow

1. `POST /api/auth/login`  
   - Super Admin: **no** `X-Tenant-ID`  
   - School users: **with** `X-Tenant-ID`
2. Store `accessToken` + `refreshToken`
3. `GET /api/auth/me` for profile/roles
4. On 401: `POST /api/auth/refresh-token` then retry
5. Logout: `POST /api/auth/revoke-token`

Role claim uses **prefix** values: `superadmin`, `admin`, `teacher`, `accountant`, `librarian`, `parent`, `student`, …

### 4.6 Files / MinIO

Upload endpoints return object keys or URLs. Display images via returned **presigned URLs**. Never hardcode MinIO bucket paths across tenants.

---

## 5. Auth & route guards (Next.js)

Implement middleware (`middleware.ts`) + server-side checks:

| Path prefix | Allowed roles |
|-------------|---------------|
| `/platform/*` | `superadmin` |
| `/portal/*` | `admin`, `accountant`, `teacher`, `librarian`, … (feature flags per nav item) |
| `/parent/*` | `parent` |
| `/student/*` | `student` |
| `/` public site | anonymous |
| `/admission/apply` | anonymous |

Hide nav items the role cannot use; still enforce on the API (never trust UI alone).

---

## 6. Shared UI patterns

Every school-portal module page should reuse:

1. **Page header** — title, primary action (Add), breadcrumbs  
2. **Filter bar** — search, class/section, date range, status  
3. **Data table** — sort, page size, row actions (View / Edit / Delete)  
4. **Drawer or modal forms** — create/edit with Zod validation  
5. **Confirm dialog** — destructive actions  
6. **Empty / loading / error** states  
7. **Export** button where API supports export (CSV/Excel/PDF)  
8. **Tenant badge** in top bar (school name + slug)

List defaults:

- Debounced search (300–400ms)
- Server-side pagination (`page`, `pageSize`)
- Optimistic UI only for toggles (active / publish); otherwise invalidate TanStack Query keys

---

## 7. Full module → pages map

Backend module docs live in `docs/modules/`. Build these **portal pages** (suggested App Router paths). Adjust labels for BN/EN.

### 01 — Authentication · [`01-auth.md`](./modules/01-auth.md)

| Page | Route | API |
|------|-------|-----|
| Login | `/login` | `POST /api/auth/login` |
| Profile | `/portal/profile` | `GET/PUT /api/auth/me` |
| Refresh (silent) | — | `POST /api/auth/refresh-token` |

### 02 — Tenants & Schools · [`02-tenants-schools.md`](./modules/02-tenants-schools.md)

| Page | Route | Who |
|------|-------|-----|
| Schools list | `/platform/schools` | Super Admin |
| Create school | `/platform/schools/new` | Super Admin |
| School detail / stats | `/platform/schools/[slug]` | Super Admin |
| School settings (platform) | `/platform/schools/[slug]/settings` | Super Admin |

APIs: `/api/tenants`, `/api/schools`

### 03 — Student Admission · [`03-student-admission.md`](./modules/03-student-admission.md)

| Page | Route |
|------|-------|
| New admission | `/portal/students/admission` |
| Edit admission | `/portal/students/[id]/edit` |

### 04 — Online Admission · [`04-online-admission.md`](./modules/04-online-admission.md)

| Page | Route | Who |
|------|-------|-----|
| Public apply | `/admission/apply` | Public |
| Track application | `/admission/track/[ref]` | Public |
| Admin inbox | `/portal/online-admissions` | Admin |
| Application detail | `/portal/online-admissions/[id]` | Admin (approve/decline/payment) |

### 05 — CSV Student Import · [`05-student-import.md`](./modules/05-student-import.md)

| Page | Route |
|------|-------|
| Import wizard | `/portal/students/import` |
| Batch result / failed rows | `/portal/students/import/[batchId]` |

### 06 — Student Categories · [`06-student-categories.md`](./modules/06-student-categories.md)

| Page | Route |
|------|-------|
| Categories CRUD | `/portal/students/categories` |

### 07 — Student List · [`07-student-list.md`](./modules/07-student-list.md)

| Page | Route |
|------|-------|
| Directory | `/portal/students` |
| Student profile | `/portal/students/[id]` |
| Deactivate / reactivate | actions on detail + list |

### 08 — Deactivate Reasons · [`08-deactivate-reasons.md`](./modules/08-deactivate-reasons.md)

| Page | Route |
|------|-------|
| Reason master | `/portal/students/deactivate-reasons` |

### 09 — Student Login Deactivate · [`09-login-deactivate.md`](./modules/09-login-deactivate.md)

| Page | Route |
|------|-------|
| Login enable/disable | `/portal/students/login-access` |

### 10 — Parents · [`10-parents.md`](./modules/10-parents.md)

| Page | Route |
|------|-------|
| Parents directory | `/portal/parents` |
| Parent detail / edit | `/portal/parents/[id]` |
| Add parent | `/portal/parents/new` |

### 11 — Parent Login Deactivate · [`11-parent-login-deactivate.md`](./modules/11-parent-login-deactivate.md)

| Page | Route |
|------|-------|
| Parent login access | `/portal/parents/login-access` |

### 12 — Employees · [`12-employees.md`](./modules/12-employees.md)

| Page | Route |
|------|-------|
| Departments | `/portal/hr/departments` |
| Designations | `/portal/hr/designations` |
| Employees list | `/portal/hr/employees` |
| Employee detail | `/portal/hr/employees/[id]` |
| Import employees | `/portal/hr/employees/import` |
| Employee login access | `/portal/hr/login-access` |

### 13 — Payroll · [`13-payroll.md`](./modules/13-payroll.md)

| Page | Route |
|------|-------|
| Salary templates | `/portal/payroll/templates` |
| Assign grades | `/portal/payroll/assign` |
| Monthly payment | `/portal/payroll/payments` |
| My salary | `/portal/payroll/my-salary` |

### 14 — Advance Salary & Leave · [`14-advance-salary-and-leave.md`](./modules/14-advance-salary-and-leave.md)

| Page | Route |
|------|-------|
| Advance requests | `/portal/payroll/advances` |
| Leave categories | `/portal/hr/leave/categories` |
| Leave applications | `/portal/hr/leave/applications` |

### 15 — Awards · [`15-awards.md`](./modules/15-awards.md)

| Page | Route |
|------|-------|
| Awards list / give award | `/portal/awards` |

### 16 — Academic · [`16-academic.md`](./modules/16-academic.md)

| Page | Route |
|------|-------|
| Classes | `/portal/academic/classes` |
| Sections | `/portal/academic/sections` |
| Subjects | `/portal/academic/subjects` |
| Class–subject assign | `/portal/academic/class-subjects` |
| Class teachers | `/portal/academic/class-teachers` |
| Class schedules | `/portal/academic/schedules` |
| Student promotion | `/portal/academic/promotion` |
| Student electives (see 35) | `/portal/academic/electives` |

### 17 — Exam Master · [`17-exam-master.md`](./modules/17-exam-master.md)

| Page | Route |
|------|-------|
| Exam terms | `/portal/exams/terms` |
| Exam halls | `/portal/exams/halls` |
| Mark distributions | `/portal/exams/distributions` |
| Exams | `/portal/exams` |
| Schedules | `/portal/exams/[id]/schedule` |
| Mark entry | `/portal/exams/[id]/marks` |

### 18 — Grades & Positions · [`18-grades-and-positions.md`](./modules/18-grades-and-positions.md)

| Page | Route |
|------|-------|
| Grade ranges | `/portal/exams/grades` |
| Generate / view positions | `/portal/exams/[id]/positions` |

### 19 — Attendance · [`19-attendance.md`](./modules/19-attendance.md)

| Page | Route |
|------|-------|
| Student daily attendance | `/portal/attendance/students` |
| Subject attendance | `/portal/attendance/subjects` |
| Employee attendance | `/portal/attendance/employees` |
| Exam attendance | `/portal/attendance/exams` |

### 20 — Library · [`20-library.md`](./modules/20-library.md)

| Page | Route |
|------|-------|
| Book categories | `/portal/library/categories` |
| Books | `/portal/library/books` |
| Issue / return | `/portal/library/issues` |
| My issues | `/portal/library/my-issues` |

### 21 — Events · [`21-events.md`](./modules/21-events.md)

| Page | Route |
|------|-------|
| Event types | `/portal/events/types` |
| Events | `/portal/events` |
| Public events (optional embed) | uses `GET /api/events/public` |

### 22 — Student Accounting · [`22-student-accounting.md`](./modules/22-student-accounting.md)

| Page | Route |
|------|-------|
| Fees types | `/portal/fees/types` |
| Fees groups | `/portal/fees/groups` |
| Fine setup | `/portal/fees/fines` |
| Allocation | `/portal/fees/allocations` |
| Invoices | `/portal/fees/invoices` |
| Offline payments | `/portal/fees/offline-payments` |
| Reminders | `/portal/fees/reminders` |

### 23 — Office Accounting · [`23-office-accounting.md`](./modules/23-office-accounting.md)

| Page | Route |
|------|-------|
| Voucher heads | `/portal/accounting/voucher-heads` |
| Accounts | `/portal/accounting/accounts` |
| Deposits | `/portal/accounting/deposits` |
| Expenses | `/portal/accounting/expenses` |
| Transactions ledger | `/portal/accounting/transactions` |

### 24 — Messages · [`24-messages.md`](./modules/24-messages.md)

| Page | Route |
|------|-------|
| Inbox | `/portal/messages/inbox` |
| Sent | `/portal/messages/sent` |
| Important | `/portal/messages/important` |
| Trash | `/portal/messages/trash` |
| Compose / thread | `/portal/messages/[id]` |

Show unread badge from unread-count API.

### 25 — Global Settings · [`25-global-settings.md`](./modules/25-global-settings.md)

| Page | Route | Who |
|------|-------|-----|
| Platform global settings | `/platform/settings/global` | Super Admin |

### 26 — School Settings · [`26-school-settings.md`](./modules/26-school-settings.md)

| Page | Route |
|------|-------|
| General / panel / payment | `/portal/settings/school` |
| Logos | `/portal/settings/school/branding` |

### 27 — Biometric (ZKTeco K40-H) · [`27-biometric-zkteco.md`](./modules/27-biometric-zkteco.md)

| Page | Route |
|------|-------|
| Devices | `/portal/biometric/devices` |
| PIN maps | `/portal/biometric/maps` |
| Punch logs | `/portal/biometric/punches` |
| Manual punch (admin) | action on punches page |

Device hardware setup is documented in module 27 — UI only manages registry/maps/logs.

### 28 — Settings · [`28-settings.md`](./modules/28-settings.md)

| Page | Route |
|------|-------|
| Roles & permissions | `/portal/settings/roles` |
| Academic sessions | `/portal/settings/sessions` |
| Cron jobs | `/portal/settings/cron` |
| DB backup / restore | `/portal/settings/backup` |
| Login log | `/portal/settings/login-log` |
| Attendance type / accounting links | `/portal/settings/school-extras` |

### 29 — Email Gateway · [`29-email-gateway.md`](./modules/29-email-gateway.md)

| Page | Route |
|------|-------|
| SMTP + templates + test | `/portal/settings/email` |

### 30 — SMS Gateway · [`30-sms-gateway.md`](./modules/30-sms-gateway.md)

| Page | Route |
|------|-------|
| BulkSMSBD + templates + test | `/portal/settings/sms` |

### 31 — Student Reports · [`31-student-reports.md`](./modules/31-student-reports.md)

| Page | Route |
|------|-------|
| Login credentials | `/portal/reports/students/credentials` |
| Admission report | `/portal/reports/students/admission` |
| Class / section | `/portal/reports/students/class-section` |
| Siblings | `/portal/reports/students/siblings` |

### 32 — Attendance Reports · [`32-attendance-reports.md`](./modules/32-attendance-reports.md)

| Page | Route |
|------|-------|
| Student attendance report | `/portal/reports/attendance/students` |
| Subject attendance report | `/portal/reports/attendance/subjects` |
| Employee attendance report | `/portal/reports/attendance/employees` |
| Exam attendance report | `/portal/reports/attendance/exams` |
| Fingerprint punch report | `/portal/reports/attendance/fingerprint` |

### 33 — HR Reports · [`33-hr-reports.md`](./modules/33-hr-reports.md)

| Page | Route |
|------|-------|
| Leave reports | `/portal/reports/hr/leave` |
| Payroll summary | `/portal/reports/hr/payroll` |

### 34 — Examination Reports · [`34-examination-reports.md`](./modules/34-examination-reports.md)

| Page | Route |
|------|-------|
| Report card | `/portal/reports/exams/report-card` |
| Tabulation sheet | `/portal/reports/exams/tabulation` |
| Progress report | `/portal/reports/exams/progress` |

Print-friendly layouts required (A4).

### 35 — Student Electives · [`35-student-electives.md`](./modules/35-student-electives.md)

| Page | Route |
|------|-------|
| Elective / additional subject assignment | `/portal/academic/electives` |

UI must enforce **Higher Math XOR Agriculture** for the 4th-subject group; Biology as additional-subject option for GPA (see module doc).

### 36 — Public Website · [`36-public-website.md`](./modules/36-public-website.md)

Build the **public school site** (ahskbera-style) as Next.js pages calling `/api/public/*` with `X-Tenant-ID`.

| Public page | Suggested route | API |
|-------------|-----------------|-----|
| Home | `/` | `GET /api/public/home` + site layout |
| History | `/history` | `GET /api/public/about/history` |
| President speech | `/president-speech` | `…/about/speeches/president` |
| Headmaster speech | `/headmaster-speech` | `…/about/speeches/headmaster` |
| Presidents list | `/presidents` | `…/leadership/presidents` |
| Headmasters list | `/headmasters` | `…/leadership/headmasters` |
| Committee | `/committee` | `…/leadership/committee` |
| Teachers | `/teachers` | `…/staff/teachers` |
| Office staff | `/office-staff` | `…/staff/office` |
| Documents | `/documents` | `GET /api/public/documents` |
| Class routine | `/class-routine` | `…/academic/routines/class-routine` |
| School exam routine | `/school-exam-routine` | `…/routines/school-exam-routine` |
| SSC exam routine | `/ssc-exam-routine` | `…/routines/ssc-exam-routine` |
| SSC vocational routine | `/ssc-vocational-exam-routine` | `…/routines/ssc-vocational-exam-routine` |
| Prospectus | `/prospectus` | `…/pages/prospectus` |
| Admission process/test/form | `/admission-*` | `…/pages/{slug}` |
| Lesson / library / lab | `/lesson-planning`, `/library`, `/laboratory` | pages |
| Golden Jubilee | `/golden-jubilee` | `…/pages/golden-jubilee` |
| Handnotes | `/handnotes` | `…/handnotes` |
| Online classes | `/online-classes` | `…/online-classes` |
| Notices | `/notices`, `/notices/[id]` | `GET /api/public/notices` |
| Gallery | `/gallery`, `/gallery/[id]` | `GET /api/public/gallery` |
| SSC results | `/ssc-exam-results` | `GET /api/public/results/ssc` |
| Result analytics | `/result-analytics` | `GET /api/public/results/analytics` |
| Student statistics | `/student-statistics` | `GET /api/public/students/statistics` |
| Student list | `/students/[class]/[section]` | `GET /api/public/students` |
| Contact | `/contact` | `GET/POST /api/public/contact` |
| Layout chrome | every page | `site/settings`, `menu`, `footer`, `visitors` |

On each public page load: hit visitors + optionally `POST …/visitors/hit`.

**Admin CMS** for website content (`/api/website/*`) may ship later — until then seed via DB or temporary admin tools.

---

## 8. Parent & student portal pages (minimum)

### Parent (`/parent`)

- Dashboard (wards summary)
- Ward profile / attendance / fees / exam results (scoped APIs only)
- Messages
- Profile

### Student (`/student`)

- Dashboard
- My profile
- My attendance
- My exam results / published schedules
- Library my-issues
- Messages
- Profile

Wire only endpoints the role is allowed to call (see each module doc).

---

## 9. Navigation IA (school portal)

Suggested sidebar groups:

1. **Dashboard**  
2. **Students** — admission, list, import, categories, online admissions, login access, parents  
3. **HR** — departments, designations, employees, leave, awards  
4. **Payroll** — templates, assign, payments, advances  
5. **Academic** — classes, sections, subjects, schedules, promotion, electives  
6. **Exams** — terms, halls, exams, marks, grades, positions  
7. **Attendance** — student / subject / employee / exam + biometric  
8. **Fees** — student accounting  
9. **Office** — office accounting  
10. **Library**  
11. **Events**  
12. **Messages**  
13. **Reports** — student / attendance / HR / examination  
14. **Settings** — school, roles, sessions, email, SMS, cron, backup, login log  

Super Admin top nav: **Schools**, **Global settings**.

---

## 10. Implementation phases

| Phase | Deliver |
|-------|---------|
| **P0** | Next.js scaffold, apiClient, auth, tenant context, layout shells |
| **P1** | Login + Super Admin schools + School dashboard shell |
| **P2** | Students (admission, list, categories, parents) |
| **P3** | HR + payroll + leave |
| **P4** | Academic + exams + marks + electives |
| **P5** | Attendance + biometric admin screens |
| **P6** | Fees + office accounting |
| **P7** | Library + events + messages |
| **P8** | All reports (print layouts) |
| **P9** | Settings (school/global/email/SMS/roles/backup) |
| **P10** | Public website (module 36) + online admission public forms |
| **P11** | Parent + student portals |

Ship each phase behind role-based nav; keep API docs as source of truth.

---

## 11. Coding conventions (frontend)

1. One feature folder per backend module; export page components + hooks + schemas.  
2. Generate or hand-maintain TypeScript types from Swagger (`openapi-typescript` optional).  
3. Never use `PUT` for updates — always `PATCH`.  
4. Never send `X-Tenant-ID` on Super Admin-only global calls unless the API requires acting inside a school.  
5. Always send `X-Tenant-ID` for public website and school portal.  
6. Centralize toast / error mapping from `ApiResponse`.  
7. Prefer Server Components for public marketing pages; Client Components for dashboards/tables.  
8. Accessible forms (labels, keyboard, focus traps in dialogs).  
9. Mobile-responsive portal (sidebar collapses).  
10. Do not commit secrets; use env vars only.

---

## 12. Local development

```bash
# Terminal 1 — API
cd SchoolManagement
make up   # or dotnet run on :5000

# Terminal 2 — Next.js
cd school-mgmt-web
pnpm install
pnpm dev   # http://localhost:3000
```

CORS: ensure API allows the Next.js origin in Development.

Swagger (contract explorer): `http://localhost:5000/swagger`

---

## 13. Testing checklist (frontend)

- [ ] Login as Super Admin without tenant header  
- [ ] Login as School Admin with `X-Tenant-ID`  
- [ ] Switching school slug never leaks previous school data (clear query cache)  
- [ ] 401 refresh rotation works  
- [ ] Role cannot open forbidden routes  
- [ ] PATCH update flows  
- [ ] File upload + presigned image display  
- [ ] Public site works with only `X-Tenant-ID`  
- [ ] Contact form + online admission submit  
- [ ] Report print preview (A4)  
- [ ] Biometric pages show punches; no browser calls to `/iclock`

---

## 14. Related docs

| Doc | Purpose |
|-----|---------|
| [`MODULES.md`](./MODULES.md) | Module index 1–36 |
| [`SYSTEM_WORKFLOW.md`](./SYSTEM_WORKFLOW.md) | End-to-end business flow |
| [`modules/36-public-website.md`](./modules/36-public-website.md) | Public API details |
| [`modules/27-biometric-zkteco.md`](./modules/27-biometric-zkteco.md) | Device + biometric admin |
| API README | Run API / Docker / Swagger |

---

## 15. Summary

- **Frontend must be Next.js (App Router) + TypeScript.**  
- Treat the API as the source of truth; map every module 01–36 to portal or public pages above.  
- Multi-tenant rule: **school data isolation via `X-Tenant-ID`**; public site included.  
- Updates are **PATCH**. Responses are **`ApiResponse<T>`**.  
- Keep this guideline under **`docs/`** — not the repository root.
