# techtalks-aidslot

## AI Defense Lab 2026 — Track 3: Offensive Security & Red Teaming

**AidSlot Security Lab** is a bounded authorization validation system for this existing NGO campaign application. It tests whether a signed-in account from one synthetic organization can read another organization's recipient list. A PowerShell checker authenticates as Organization A, requests A's campaign as a normal-access control, requests B's campaign as an adversarial case, and reports the HTTP status and presence of B's unique synthetic canary. The result is `VULNERABLE`, `PROTECTED`, or `INCONCLUSIVE`; an unexpected page is not treated as proof of either outcome.

**Security control:** ASP.NET Identity supplies the current user. Campaign reads and writes resolve that user's `OrganizationId` and require the campaign to belong to that same organization before returning data or changing receipt status. The checker runs against an explicit localhost URL only. The deliberate weak read mode is off by default and restricted to Development, loopback, and the exact synthetic lab organizations; it exists to demonstrate positive detection. Do not enable it against real data.

**Reproduce the demo:** Follow [the local lab setup](security_lab/SETUP.md), using a separate PostgreSQL database and the provided fictional CSV records. Run `security_lab/check_access.ps1` in two configurations: weak mode enabled should detect the B canary, then weak mode disabled should show A's own campaign as HTTP 200 and B's as HTTP 404. The [evaluation record](security_lab/EVALUATION.md) contains the observed results and limitations. No Python installation is needed for the PowerShell checker.

**Work disclosure:** AidSlot's original campaign, CSV import, Identity, and check-in functionality predate this hackathon. The two-organization model, authorization checks, local synthetic seeder, bounded checker, and evaluation were added for the event. The lab uses only synthetic recipients and a participant-owned local application. It does not claim a proven cross-organization exploit in the original one-organization version.

**Architecture:** browser or checker → ASP.NET Core MVC and Identity → server-side organization ownership check → EF Core/PostgreSQL → allow the owner's campaign or return 404. The checker's verdict uses both response status and the unique B canary. It covers one recipient-list route; broader endpoint review and audit logging remain future work.

# Project Overview : 

AidSlot is a lightweight aid-distribution management system designed for small NGOs in Lebanon.

The project aims to make distribution days more organized and efficient by helping NGOs coordinate recipient time slots, manage check-ins, prevent duplicate collections, monitor stock during distribution, and generate a final distribution report.

## Approved MVP
Recipient CSV import
Time-slot assignment
Unique QR code generation for recipients
Manual name or code lookup as a fallback to QR scanning
Duplicate collection prevention
Simple live stock counter
Final distribution report

## Tech Stack
### Backend :
C#
ASP.NET Core
### Architecture :
ASP.NET Core MVC
### Database :
PostgreSQL
### QR Code Generation : 
QRCoder

## Team & Responsibilities 

## Getting started 

Local Database Setup

1. Install PostgreSQL.
2. Create a local database named aidslot_dev.
3. From the AidSlot project folder run:

   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=aidslot_dev;Username=postgres;Password=YOUR_PASSWORD"

4. Apply migrations:

   dotnet ef database update

5. Run:

   dotnet run


## Contribution Workflow

To keep development organized and maintain code quality, all team members should follow the contribution workflow below.

1. Create a Branch
### 1. Create a Branch

Do not develop new features directly on the main branch.


feature/qr-check-in
feature/time-slot-assignment

2. Write Meaningful Commits
### 2. Write Meaningful Commits

Commits should clearly describe the change being made.

### 3. Open a Pull Request

When a feature or task is ready for review:

Push the branch to the remote repository.
Open a Pull Request (PR).
Clearly describe what was implemented or changed.
Mention any important implementation decisions or known limitations.
