# 23 — Office Accounting

Voucher heads, accounts, deposits, expenses, combined transactions.

**Headers:** `Authorization: Bearer {token}` + `X-Tenant-ID: {slug}`  
**Updates:** `PATCH` (no PUT)

**Seed voucher heads:** School Fees & Government Grant (Income); Salary, Utilities, Stationery (Expense)

## Routes (`/api/office-accounting/...`)

| Area | Prefix | Notes |
|------|--------|-------|
| Voucher heads | `/voucher-heads` | CRUD + `/lookup?type=` |
| Accounts | `/accounts` | Opening balance → current balance |
| Deposits | `/deposits` | += balance; MinIO attachment |
| Expenses | `/expenses` | -= balance; MinIO attachment |
| Transactions | `/transactions` | Branch, Type, Dr/Cr, Running Balance; filter account/date/type/voucher |

## Balance rules

- Create deposit → `current_balance += amount`  
- Create expense → `current_balance -= amount`  
- Update/delete reverses previous amount then applies new  

## Auth

Admin / Super Admin / Accountant only
