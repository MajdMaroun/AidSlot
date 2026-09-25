# AidSlot authorization lab: initial evaluation

## Scope

Local development instance at `http://localhost:5204`, synthetic organizations A and B, synthetic campaigns 1 and 3. The test authenticates as Organization A and makes two bounded read requests to `Campaign/Recipients/{id}`. A unique fictional name, `Canary Recipient Bravo 8472`, identifies Organization B's response. No real recipient data or external target was used.

## Observed results

| Configuration | Request as A | HTTP status | B canary in response | Decision |
| --- | --- | ---: | --- | --- |
| Intentionally weak lab read path enabled | A campaign 1 | 200 | No | Legitimate access works |
| Intentionally weak lab read path enabled | B campaign 3 | 200 | Yes | Vulnerable: cross-organization read confirmed |
| Lab weak read path disabled | A campaign 1 | 200 | No | Legitimate access preserved |
| Lab weak read path disabled | B campaign 3 | 404 | No | Protected: cross-organization read blocked |

The 200 response alone was treated as inconclusive when a previous B campaign did not contain the expected canary. The verdict became positive only after the synthetic B canary appeared. This avoids claiming a leak from an unexpected 200 page without matching evidence.

## Mechanism and remediation

The lab switch `AIDSLOT_LAB_VULNERABLE_READ=SYNTHETIC_LOCAL_ONLY` deliberately skips the organization boundary on one read path, only in Development, for loopback requests and the exact synthetic organization names. With the switch unset, the server checks the campaign's `OrganizationId` against the authenticated user's `OrganizationId` before returning the recipient page. Related detail and check-in actions also enforce that boundary. The checker uses a single login and two read requests and reports status plus canary presence without logging recipient records or passwords.

## Remaining evaluation before submission

- Anonymous browser request to B recipient page: observed redirect to Login in Incognito; no recipient data shown.
- Nonexistent campaign ID `/Campaign/Recipients/99999`: observed 404 while signed in as A.
- Incorrect canary: inconclusive, not a false claim of protection or vulnerability.
- Cross-organization write: verify a synthetic check-in cannot be changed by A for B, with a valid anti-forgery token.
- Role test: Volunteer sees only permitted recipient fields.
- Operational limitation: one checker route and one controlled scenario do not prove all application endpoints secure; production needs broader authorization coverage and audit logging.

The anonymous and nonexistent-ID checks were observed manually in the browser; the automated checker currently covers the two authenticated read requests above. The before/after browser screenshots show the synthetic B record and later HTTP 404. The screenshot of the command clearing the lab switch records the configuration change, while the 404 response and checker output establish the protected result.

## Browser evidence

1. [Synthetic B recipient visible to signed-in A in the deliberate weak mode](evidence/01-synthetic-cross-org-read.png).
2. [Command clearing the weak-mode switch](evidence/02-disable-demo-mode.png).
3. [The same B campaign URL returning HTTP 404 in protected mode](evidence/03-cross-org-404.png).

The first screenshot alone does not show the account identity; the authenticated A session and verdict were confirmed by the checker output recorded above. These screenshots are evidence for the local test fixture, not for an untested exploit in the original AidSlot version.

## Disclosure

AidSlot's original campaign, CSV, Identity and check-in features existed before the hackathon. The organization boundary, bounded authorization checker, synthetic lab setup and this evaluation are new hackathon work. The weak mode is a deliberate test fixture; keep it disabled outside the local demonstration.
