# 26 — School Settings

Per-tenant `school_settings` row (general, student/parent panel, payment gateways, logos).

**Headers:** Bearer + `X-Tenant-ID` for school admin; Super Admin can address any `{tenantSlug}`  
**Updates:** `PATCH` (no PUT)

**Provisioner:** creates one `school_settings` row per tenant

## Routes (`/api/settings/school`)

| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | School list (Super Admin) |
| GET | `/{tenantSlug}` | Full settings for school |
| PATCH | `/{tenantSlug}/general` | School details / currency / register prefix |
| PATCH | `/{tenantSlug}/student-panel` | Login visibility flags |
| PATCH | `/{tenantSlug}/payment` | Gateway JSON + active_gateways |
| POST | `/{tenantSlug}/logo` | Upload system/text/printing/report-card logos |

Payment gateways stored as JSONB (`payment_gateways` + `active_gateways`).  
Logos → MinIO `settings/logos/{type}.*`
