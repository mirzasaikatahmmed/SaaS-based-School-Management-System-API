# 31 — Student Reports (Login Credential, Admission, Class & Section, Sibling)

Student report screens matching the portal **Reports → Student Reports** menu.

**Headers:** Bearer + `X-Tenant-ID`  
**Updates:** `PATCH` not used (reports are GET; password reset is POST)  
**Base path:** `/api/reports/students`

---

## Login Credential (with passwords)

Shows student + parent usernames **and passwords**, plus photo, class, section,
register no, roll, guardian name, and a reset action.

| Method | Path | Permission |
|--------|------|------------|
| GET | `/login-credentials?classId=&sectionId=&search=&page=&pageSize=&export=csv` | `Reports.StudentLoginCredential` View |
| POST | `/login-credentials/{studentId}/reset-password` | `Reports.StudentLoginCredential` Edit |

### Password columns

Login passwords are stored as bcrypt hashes and cannot be reversed. To support the
credential report (same UX as ahskbera), the API keeps a **separate encrypted reveal**
on `users.password_reveal_encrypted` (ASP.NET Data Protection) whenever an admin
sets a password at admission / import / parent create / **reset**.

| Column | Source |
|--------|--------|
| `studentUsername` | Student user account |
| `studentPassword` | Decrypted reveal (null if never stored — e.g. pre-migration accounts) |
| `parentUsername` | Primary guardian user account |
| `parentPassword` | Decrypted reveal for guardian |

Response includes:

```json
{
  "note": "Passwords are shown from the last value set by an admin (admission/reset). Accounts created before password-reveal storage show blank until reset.",
  "data": [
    {
      "studentId": "…",
      "name": "MST SAYMA KHATUN",
      "studentUsername": "20266001",
      "studentPassword": "abc123",
      "parentUsername": "g20266001",
      "parentPassword": "parent123",
      "passwordRevealAvailable": true
    }
  ]
}
```

### Reset password

```http
POST /api/reports/students/login-credentials/{studentId}/reset-password
{
  "newPassword": "optional",
  "resetParentPassword": true,
  "newParentPassword": "optional"
}
```

Omitting passwords generates random temporary passwords. The response returns the
new plaintext values once so they can be printed/shared.

---

## Admission Report

| Method | Path |
|--------|------|
| GET | `/admission?classId=&sectionId=&fromDate=&toDate=&search=&page=&pageSize=&export=csv` |

Returns a summary line (`Total of N students Admission during this period…`) and
rows: name, gender, register no, roll, class, section, guardian, admission date.

Permission: `Reports.StudentAdmission` View.

---

## Class & Section Report

| Method | Path |
|--------|------|
| GET | `/class-section` |

Rows: class name, sections with counts (`A (60)`), total students.

Permission: `Reports.StudentClassSection` View.

---

## Sibling Report

| Method | Path |
|--------|------|
| GET | `/siblings?classId=&sectionId=&search=` |

Groups students who share a guardian login (`UserId`) or the same guardian
mobile + name (2+ students). Columns: guardian name, mobile, father/mother,
occupation, sibling list (name, register no, class, gender).

Permission: `Reports.StudentSibling` View.

---

## Migration

Tenant: `20260809204404_AddPasswordRevealAndStudentReports`  
(`users.password_reveal_encrypted`; also applied via `EnsureSettingsModuleAsync`)
