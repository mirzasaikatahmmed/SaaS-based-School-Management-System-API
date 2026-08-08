# 5 — CSV Student Import

**Controller:** `StudentImportController` · **Route:** `/api/student-import`  
**Roles:** Super Admin, Admin

Row-by-row import with partial success. Failed rows can be downloaded; CSV stored under MinIO `imports/{batchId}/…`.

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/student-import/sample-csv` | Download sample template |
| POST | `/api/student-import` | Upload CSV (class + section required) |
| GET | `/api/student-import/batches` | List import batches |
| GET | `/api/student-import/batches/{batchId}` | Batch summary |
| GET | `/api/student-import/batches/{batchId}/errors` | Failed rows export |

## Workflow

1. Download sample CSV and fill rows.
2. Choose class/section → upload.
3. Review batch success/fail counts.
4. Fix failed rows via errors export and re-import if needed.

## Notes

- Uses CsvHelper; each valid row creates a student (and guardian when columns present).
- Per-row transactions so one bad row does not roll back the whole file.
