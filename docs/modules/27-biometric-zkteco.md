# 27 — Biometric Attendance (ZKTeco K40-H / ADMS)

Multi-device ZKTeco K40-H fingerprint attendance integration over the ADMS push protocol.
Devices push punches to the server in real time; the backend maps each device PIN to a
student or employee and auto-fills **Student Attendance**, **Employee Attendance**, and
(when the punch falls inside a scheduled exam window) **Exam Attendance**.

**Headers (Admin APIs):** Bearer + `X-Tenant-ID` for school admin
**Updates:** `PATCH` (no PUT)
**Device endpoints (`/iclock/*`):** `AllowAnonymous`, plain text (`text/plain`), no `X-Tenant-ID` — tenant is resolved server-side from the device serial number.

**Provisioner:** `EnsureBiometricModuleAsync` (chains from `EnsureMessageAndSettingsModuleAsync`) creates `biometric_devices`, `biometric_user_maps`, `biometric_punch_logs` per tenant schema. No devices are seeded — the school admin registers each physical unit after provisioning.

## Data model

| Table | Scope | Purpose |
|-------|-------|---------|
| `public.biometric_device_registry` | Master DB | Global SN → tenant lookup (SN is unique across all schools) so an anonymous ADMS request can be routed without `X-Tenant-ID` |
| `biometric_devices` | Tenant schema | Device config (name, location, grace minutes, active flag) |
| `biometric_user_maps` | Tenant schema | `DevicePin` → `StudentId` **or** `EmployeeId` (exactly one), unique PIN per tenant |
| `biometric_punch_logs` | Tenant schema | Every raw punch received, plus what it resolved to (`PunchKind`, matched exam/subject) |

## Admin routes

### `/api/biometric/devices`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | List devices for current tenant |
| GET | `/{id}` | Single device |
| POST | `/` | Register a device (`Name`, `SerialNumber`, `Location?`, `DeviceModel?` default `K40-H`, `ExamGraceMinutesBefore/After` default 30). Also upserts the Master `biometric_device_registry` row (SN unique globally) |
| PATCH | `/{id}` | Update name/location/grace minutes/active flag; syncs `DeviceName` in the registry |
| DELETE | `/{id}` | Removes tenant device + registry row |

### `/api/biometric/maps`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | List PIN mappings (with student/employee names) |
| GET | `/{id}` | Single mapping |
| POST | `/` | Create mapping. `PersonType` = `Student` or `Employee`; exactly one of `StudentId`/`EmployeeId` required; `DevicePin` unique per tenant |
| PATCH | `/{id}` | Change `DevicePin` and/or `IsActive` |
| DELETE | `/{id}` | Remove mapping |

### `/api/biometric/punches`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | Filter by `from`, `to`, `deviceId`, `kind` (`Unmapped`/`StudentDaily`/`EmployeeDaily`/`Exam`), paginated |
| POST | `/manual` | Record a punch without hardware, for testing (`SerialNumber`, `DevicePin`, `PunchTime?`) — Admin/SuperAdmin/Accountant only |

**Roles:** `SuperAdmin`, `Admin`, `Accountant` manage devices/maps and record manual punches. `Teacher` can additionally view punch logs (`GET /api/biometric/punches`).

## ADMS device protocol (`/iclock/*`)

Firmware is configured with **Cloud Server URL = `http://{api-host}:{port}`** (no path — the device itself appends `/iclock/...`). No credentials, no `X-Tenant-ID`; every device is uniquely identified by its serial number (`SN` query param), which is looked up in the Master `biometric_device_registry` to resolve the tenant schema.

| Method | Path | Behavior |
|--------|------|----------|
| GET | `/iclock/cdata?SN=&options=all` | Handshake — returns option block seeded with `Stamp` from the registry's `AttLogStamp` |
| POST | `/iclock/cdata?SN=&table=ATTLOG&Stamp=` | Body = one punch per line, tab-separated (`PIN\tYYYY-MM-DD HH:MM:SS\t...`). Each line is processed and persisted; `Stamp` is saved back to the registry so the device doesn't resend old records |
| POST | `/iclock/cdata?SN=&table=OPERLOG` | Accepted, not processed (enroll/delete admin events) |
| GET | `/iclock/getrequest?SN=` | Always `OK` (no queued remote commands) |
| POST | `/iclock/devicecmd` | Always `OK` |
| GET/POST | `/iclock/registry?SN=` | Always `OK` |

An **unknown serial number** never breaks the device — the handler logs a warning and still answers `OK` / a default handshake block, so the K40-H keeps functioning (and simply won't get its punches applied) until an admin registers it.

### Handshake sample (known device)

```
GET OPTION FROM: K40H-TEST-001
Stamp=0
OpStamp=0
ErrorDelay=30
Delay=30
TransFlag=1111111111
TransInterval=1
Realtime=1
Encrypt=0
```

## Punch processing logic

For every `ATTLOG` line (`DevicePin`, `PunchTime`):

1. A `BiometricPunchLog` row is always created (`PunchKind` defaults to `Unmapped`).
2. Look up an **active** `BiometricUserMap` by `DevicePin`.
   - No mapping → log stays `Unmapped`, nothing else happens.
3. **Student mapping:**
   - Look for an `ExamScheduleSubject` for the student's class/section on the punch date whose window `[StartingTime − graceBefore, EndingTime + graceAfter]` (device grace minutes, default 30) contains the punch time. If several match, the closest to `StartingTime` wins.
   - Match found → upsert `ExamAttendance` (`Status = Present`, `Remarks = "ZKTeco {SN}"`), `PunchKind = Exam`.
   - Always also upsert daily `StudentAttendance` (`Status = Present`) for that date; `PunchKind = StudentDaily` if no exam matched.
4. **Employee mapping:** upsert `EmployeeAttendance` (`Status = Present`) for that date; `PunchKind = EmployeeDaily`.
5. All attendance upserts are idempotent per (person, date) — repeated punches on the same day don't create duplicate rows.

## Device setup (physical K40-H)

1. On the device: **Menu → Comm → Cloud Server Setting** → Server Address = API host, Server Port = API port, Enable Domain Name = off. Leave the path blank; firmware always calls `/iclock/...`.
2. Note the device serial number from **Menu → System Info → Device Info**.
3. In the admin panel, `POST /api/biometric/devices` with that `SerialNumber` for each of the 3 units (e.g. Main Gate, Staff Room, Exam Hall).
4. Enroll fingerprints on the device with a numeric PIN per person, then `POST /api/biometric/maps` to link each PIN to a `StudentId` or `EmployeeId`.
5. Punches now flow automatically; verify via `GET /api/biometric/punches`.
