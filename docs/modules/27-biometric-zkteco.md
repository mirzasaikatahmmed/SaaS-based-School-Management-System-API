# 27 — Biometric Attendance (ZKTeco K40-H / ADMS)

Multi-device ZKTeco K40-H fingerprint attendance over the **ADMS push** protocol.
Devices push punches to the server in real time; the backend maps each device PIN to a
student or employee and auto-fills **Student Attendance**, **Employee Attendance**, and
(when the punch falls inside a scheduled exam window) **Exam Attendance**.

| | |
|--|--|
| **Admin APIs** | Bearer + `X-Tenant-ID` · updates use `PATCH` (no PUT) |
| **Device endpoints** | `/iclock/*` · `AllowAnonymous` · plain text · no `X-Tenant-ID` |
| **Tenant routing** | Device serial number (`SN`) → Master `biometric_device_registry` → school schema |
| **Provisioner** | `EnsureBiometricModuleAsync` — creates tables per tenant; devices are **not** seeded |

---

## Architecture

```
K40-H (fingerprint)
    │  ADMS push (no JWT / no X-Tenant-ID)
    ▼
http://{api-host}:{port}/iclock/...
    │  SN → biometric_device_registry → tenant_{slug}
    ▼
biometric_punch_logs
    │  active PIN map?
    ├─ Student  → Student Attendance (+ Exam Attendance if in window)
    └─ Employee → Employee Attendance
```

- One SN is unique **across all schools** (Master registry).
- PIN maps are unique **per school** (tenant schema).
- Files / attendance never cross tenants.

---

## Data model

| Table | Scope | Purpose |
|-------|-------|---------|
| `public.biometric_device_registry` | Master DB | Global SN → tenant lookup so anonymous ADMS requests can be routed |
| `biometric_devices` | Tenant schema | Name, location, model (`K40-H`), exam grace minutes, active flag, `LastSeenAt` |
| `biometric_user_maps` | Tenant schema | `DevicePin` → `StudentId` **or** `EmployeeId` (exactly one) |
| `biometric_punch_logs` | Tenant schema | Raw punch + resolved `PunchKind` / matched exam |

---

## Prerequisites

| Item | Requirement |
|------|-------------|
| API reachable | Device can open `http://API_HOST:PORT` (LAN, VPN, or public IP) |
| Protocol | Prefer **HTTP**. Many K40-H firmwares struggle with HTTPS |
| Port | e.g. `5000` or your reverse-proxy port — must be open from the device |
| Time | Device clock ≈ server time (wrong clock → wrong attendance dates) |
| Admin access | School Admin JWT + `X-Tenant-ID: {school-slug}` to register/map |

**Cloud Server URL on the device = host only** (example: `192.168.1.50`, port `5000`).  
Do **not** put `/iclock` in the URL — firmware appends `/iclock/...` itself.

---

## Physical device setup (K40-H)

### 1. Power & network

1. Power on the unit and connect Ethernet (or Wi‑Fi if supported).
2. Set a **static IP** on the same LAN as the API (recommended).

Typical menu: **Menu → Comm. → Ethernet / Network**  
Set IP, Subnet, Gateway, DNS.

### 2. Read the Serial Number

**Menu → System → Device Info** (or System Info)

Copy the **Serial Number** exactly. You need it for SMS registration.

### 3. Cloud / ADMS server

**Menu → Comm. → Cloud Server Setting** (labels may be “ADMS”, “Cloud Server”, or “Web Server”)

| Setting | Value |
|---------|--------|
| Cloud Server / Enable | **ON** |
| Server Address | API host only — `192.168.1.50` or `api.yourschool.com` |
| Server Port | e.g. `5000` |
| Domain Name | **OFF** when using a raw IP |
| Path / URL suffix | leave **empty** |
| HTTPS | **OFF** unless you verified firmware support |

Save and reboot if prompted.

### 4. Date & time

**Menu → System → Date Time**

- Set correct local time (Bangladesh: GMT+6).
- Use NTP if available; otherwise sync manually with the server.

### 5. Harden the device

- Set a device admin password so students cannot change Comm settings.
- Clear leftover test punches before go-live if needed.

---

## Register the device in SMS

As School Admin / SuperAdmin / Accountant:

```http
POST /api/biometric/devices
Authorization: Bearer {token}
X-Tenant-ID: {school-slug}
Content-Type: application/json

{
  "name": "Main Gate",
  "location": "Front entrance",
  "serialNumber": "CEK9400001234",
  "deviceModel": "K40-H",
  "examGraceMinutesBefore": 30,
  "examGraceMinutesAfter": 30
}
```

This creates the tenant `biometric_devices` row and upserts Master `biometric_device_registry`.

Repeat for every physical unit (Main Gate, Staff Room, Exam Hall, …) with **different SNs**.

```http
GET /api/biometric/devices
```

`lastSeenAt` updates after the device successfully talks to `/iclock`.

| Method | Path | Notes |
|--------|------|-------|
| GET | `/api/biometric/devices` | List |
| GET | `/api/biometric/devices/{id}` | Detail |
| POST | `/api/biometric/devices` | Register (+ registry upsert) |
| PATCH | `/api/biometric/devices/{id}` | Name / location / grace / active |
| DELETE | `/api/biometric/devices/{id}` | Remove tenant device + registry row |

---

## Enroll people on the K40-H

1. **Menu → User Mgt → New User** (or Enroll)
2. Assign a numeric **PIN** (User ID), e.g. `1001`
3. Capture fingerprint(s) and save

Guidelines:

- One PIN = one person
- PIN must match SMS `DevicePin`
- Prefer a stable scheme (register no / employee code) — still unique **per school**
- Enrollment can be on any registered device; attendance resolves by PIN

---

## Map PIN → Student / Employee

```http
POST /api/biometric/maps
Authorization: Bearer {token}
X-Tenant-ID: {school-slug}
Content-Type: application/json

{
  "devicePin": "1001",
  "personType": "Student",
  "studentId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
}
```

Employee:

```json
{
  "devicePin": "2001",
  "personType": "Employee",
  "employeeId": "ffffffff-1111-2222-3333-444444444444"
}
```

Rules: exactly one of `studentId` / `employeeId`; `devicePin` unique per school; inactive maps leave punches as `Unmapped`.

| Method | Path | Notes |
|--------|------|-------|
| GET | `/api/biometric/maps` | List (with names) |
| GET | `/api/biometric/maps/{id}` | Detail |
| POST | `/api/biometric/maps` | Create |
| PATCH | `/api/biometric/maps/{id}` | Change PIN / `isActive` |
| DELETE | `/api/biometric/maps/{id}` | Remove |

**Roles:** `SuperAdmin`, `Admin`, `Accountant` manage devices/maps. `Teacher` can view punch logs.

---

## Day-to-day operations

### At the gate

1. Person places finger on the sensor.
2. Device shows success.
3. Punch is pushed to `/iclock/cdata` (realtime).
4. SMS marks attendance for that school only.

### Verify punches

```http
GET /api/biometric/punches?from=2026-08-10&to=2026-08-10
```

Query filters: `deviceId`, `kind` = `Unmapped` | `StudentDaily` | `EmployeeDaily` | `Exam`, plus pagination.

### Manual test punch (no hardware)

```http
POST /api/biometric/punches/manual
Authorization: Bearer {token}
X-Tenant-ID: {school-slug}
Content-Type: application/json

{
  "serialNumber": "CEK9400001234",
  "devicePin": "1001",
  "punchTime": "2026-08-10T08:05:00Z"
}
```

### Person leaves / PIN changes

1. Deactivate map: `PATCH /api/biometric/maps/{id}` with `"isActive": false` (or DELETE).
2. Optionally delete the user on the device.
3. Create a new map for the new PIN.

### Decommission a device

```http
DELETE /api/biometric/devices/{id}
```

---

## First-punch connectivity checklist

1. From a PC: open `http://API_HOST:PORT/swagger` — API is up.
2. Handshake test:

```bash
curl "http://API_HOST:PORT/iclock/cdata?SN=YOUR_SN&options=all"
```

- Known SN → option block with `Stamp=...`
- Unknown SN → still answers (device keeps working); punches are **not** applied until registered

3. Wait 1–2 minutes after Cloud settings; confirm ADMS/online icon if firmware shows one.
4. Punch once → `GET /api/biometric/punches` shows a new row.
5. Row `Unmapped` → fix PIN mapping.
6. No row → network / wrong host-port / SN not registered / firewall.

---

## Punch processing logic

For every `ATTLOG` line (`DevicePin`, `PunchTime`):

1. Always insert `BiometricPunchLog` (`PunchKind` defaults to `Unmapped`).
2. Look up an **active** `BiometricUserMap` by PIN.  
   - None → stay `Unmapped`.
3. **Student**
   - Find `ExamScheduleSubject` for the student’s class/section on that date whose window  
     `[StartingTime − graceBefore, EndingTime + graceAfter]` contains the punch.  
     Closest to `StartingTime` wins if several match.
   - Match → upsert `ExamAttendance` (`Present`, remarks `ZKTeco {SN}`), `PunchKind = Exam`.
   - Always upsert daily `StudentAttendance` (`Present`); if no exam match → `PunchKind = StudentDaily`.
4. **Employee** → upsert `EmployeeAttendance` (`Present`), `PunchKind = EmployeeDaily`.
5. Attendance upserts are idempotent per (person, date) — repeat punches same day do not duplicate rows.

| Condition | `PunchKind` |
|-----------|-------------|
| PIN not mapped / inactive | `Unmapped` |
| Student, no exam window | `StudentDaily` |
| Student, inside exam ± grace | `Exam` |
| Employee | `EmployeeDaily` |

---

## ADMS protocol (`/iclock/*`)

Firmware Cloud Server = `http://{api-host}:{port}` (no path). Routes are fixed in firmware.

| Method | Path | Behavior |
|--------|------|----------|
| GET | `/iclock/cdata?SN=&options=all` | Handshake — option block; `Stamp` from registry `AttLogStamp` |
| POST | `/iclock/cdata?SN=&table=ATTLOG&Stamp=` | Body = lines `PIN\tYYYY-MM-DD HH:MM:SS\t...`; process + save stamp |
| POST | `/iclock/cdata?SN=&table=OPERLOG` | Accepted, not processed |
| GET | `/iclock/getrequest?SN=` | Always `OK` |
| POST | `/iclock/devicecmd` | Always `OK` |
| GET/POST | `/iclock/registry?SN=` | Always `OK` |

Unknown SN: handler logs a warning and still returns `OK` / a default handshake so the device does not brick itself.

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

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| Device never online | Wrong IP/port, firewall, HTTPS | Use HTTP host+port; open firewall |
| No punches in SMS | SN not registered for this school | `POST /api/biometric/devices` with exact SN |
| Punches `Unmapped` | PIN not mapped or inactive | Create / activate map |
| Wrong attendance date | Device clock wrong | Set Date/Time |
| SN conflict | SN already used by another school | One SN = one school only |
| Exam not marked | Outside schedule ± grace, or no schedule | Adjust grace or exam schedule |
| Worked then stopped | Network / API down | Check `lastSeenAt`; reboot device; verify API |

---

## Go-live checklist

1. Mount device + network + correct time  
2. Configure Cloud Server (host + port only)  
3. Register SN in SMS for that school  
4. Confirm handshake / `lastSeenAt`  
5. Enroll 2–3 test users (1 student, 1 employee)  
6. Map PINs → verify punches + attendance  
7. Enroll remaining users + create maps  
8. Train gate staff: dry finger, one retry, call office if Unmapped  

---

## API quick reference

| Action | Endpoint |
|--------|----------|
| Register device | `POST /api/biometric/devices` |
| List devices | `GET /api/biometric/devices` |
| Map PIN | `POST /api/biometric/maps` |
| View punches | `GET /api/biometric/punches` |
| Manual punch | `POST /api/biometric/punches/manual` |
| Device protocol | `GET/POST /iclock/cdata`, `GET /iclock/getrequest`, … |

Admin calls: **Bearer + `X-Tenant-ID`**.  
Device calls: **SN query param only**.
