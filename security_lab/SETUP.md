# AidSlot two-organization security lab

This is a local, synthetic-data demonstration. The original AidSlot campaign, CSV, Identity, and check-in features predate the hackathon. The organization boundary, scoped authorization, lab seeder, and authorization checker are new work. Do not point this lab at a production database or real recipient data.

## Prepare a separate local database

Use a fresh PostgreSQL database. The project includes the new organization migration. Configure `ConnectionStrings:DefaultConnection` for that database through .NET user secrets or local environment variables; do not commit credentials. In the project directory run:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=aidslot_security_lab;Username=YOUR_USER;Password=YOUR_LOCAL_PASSWORD" --project AidSlot
```

Create the empty `aidslot_security_lab` database in pgAdmin first. Then run:

```powershell
dotnet ef database update --project AidSlot
dotnet build
```

The included migration adds `Organizations` plus nullable `OrganizationId` foreign keys on `AspNetUsers` and `Campaigns`. Existing rows deliberately remain unassigned and cannot be accessed through the new campaign actions until explicitly assigned. Never run these steps against a database holding real recipients.

## Create two synthetic organizations and accounts

In PowerShell, set unique synthetic email addresses and strong local passwords. These variables are read only when development mode and the explicit seed switch are both set:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:AIDSLOT_LAB_SEED = 'SYNTHETIC_LOCAL_ONLY'
$env:AIDSLOT_LAB_A_EMAIL = 'lab-a@example.test'
$env:AIDSLOT_LAB_A_PASSWORD = '<a unique local password>'
$env:AIDSLOT_LAB_B_EMAIL = 'lab-b@example.test'
$env:AIDSLOT_LAB_B_PASSWORD = '<another unique local password>'
dotnet run --project AidSlot
```

The seeder creates `Synthetic Organization A/B` and one Admin user in each. It never reassigns an existing account. Remove the seed switch after the first run. Use the normal UI to sign in as each account and create one campaign, uploading `security_lab/recipients_A.csv` for A and `security_lab/recipients_B.csv` for B. Both files contain only fictional data; B's unique canary is `Canary Recipient Bravo 8472`.

## Run the bounded checker

In a second Windows CMD window, use the local port shown by `dotnet run` and the two campaign IDs shown in the URLs. The canary must appear on B's recipient list and not A's. Set the synthetic A password only in this local CMD session; never commit it.

```bat
set "AIDSLOT_BASE_URL=http://localhost:5204"
set "AIDSLOT_A_EMAIL=lab-a@example.test"
set "AIDSLOT_A_PASSWORD=<your synthetic A password>"
set "AIDSLOT_A_CAMPAIGN_ID=1"
set "AIDSLOT_B_CAMPAIGN_ID=3"
set "AIDSLOT_B_CANARY=Canary Recipient Bravo 8472"
powershell -NoProfile -ExecutionPolicy Bypass -File "security_lab\check_access.ps1"
```

Use the actual URL and IDs from your local session; `3` is only the example B campaign ID from the recorded run. The PowerShell checker makes a login request and two read requests, and logs no password or recipient record. A `PROTECTED` result requires legitimate access to A and a blocked 403/404 response for B. An unexpected response is `INCONCLUSIVE`, not a claim of safety. Python is not required.

## Demo and evaluation

The `Recipients` action contains an intentionally weak read path for the *synthetic local lab only*. It is off by default. To show a positive detection, stop the app, set `AIDSLOT_LAB_VULNERABLE_READ=SYNTHETIC_LOCAL_ONLY` in the **app's** CMD window, and restart `dotnet run --project AidSlot`. Run the checker from another CMD window: it should report `VULNERABLE` and show that the B canary appeared. Stop the app again, clear the switch with `set AIDSLOT_LAB_VULNERABLE_READ=`, restart, and rerun the checker: it should report `PROTECTED` with a 404 for B. The weak path requires development mode, a loopback request, and the two exact synthetic organization names. It does not apply to write actions. Never deploy with the switch enabled.

The original AidSlot branch has no organization model, so describe its exposed campaign route accurately as pre-existing context rather than claiming it had a proven cross-organization flaw. Test: A's own campaign, B's campaign, anonymous access, missing campaign ID, and an incorrect canary. The write boundary (`ConfirmReceived`) also requires a same-organization check; demonstrate this only with a synthetic record and a valid anti-forgery token through the UI.

Limitations: this checker covers one recipient-list route and one synthetic scenario; it does not prove every endpoint safe. Review additional controllers, exports, QR links, and audit logging before production use.
