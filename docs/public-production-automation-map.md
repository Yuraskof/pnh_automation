# Public Production Automation Map

Last reviewed: 2026-04-25

This document maps the public user journeys for PilkaNaHali.pl and defines what can and cannot be automated against live production. The goal is simple: production tests may prove that the public site is usable, but they must not book real places, move money, send real messages, change real event operations, or expose personal data.

## Sources Checked

- Public site: https://www.pilkanahali.pl/
- Public terms PDF: https://pnh-public.s3.eu-central-1.amazonaws.com/regulamin.pdf

The public home page is a JavaScript application, so meaningful UI automation should use a real browser runner such as Playwright. The terms describe the service scope: event browsing, account registration, login, event participation, prepaid money, deposits, benefit partners, notifications, groups, organizers, and complaints.

## Production Safety Rule

Use this rule before adding any production automation:

> A live production test is allowed only when it is read-only, uses a dedicated test account, or stops before a real side effect.

Examples:

- Safe: open the site, browse public events, open event details, log in with a dedicated test user, then log out.
- Safe with a stop point: reach the event sign-up summary and assert the user sees the final confirmation button, but do not click it.
- Unsafe: confirm a booking, top up prepaid balance, pay through a real payment provider, cancel a real booking, send invitations, or submit a complaint to support.

## User Journey Map

| Journey | What the user is trying to do | Public automation value | Main live-production risks | Production boundary |
|---|---|---|---|---|
| Anonymous site visit | Open the site and see that the app loads. | Confirms basic availability, JavaScript boot, routing, and browser compatibility. | False failures from temporary CDN/API issues; too many checks can look like bot traffic. | Allowed as a low-frequency smoke test. Do not scrape aggressively. |
| Browse public events | Search or filter available sport events and compare dates, locations, prices, level, status, or capacity where visible. | Proves the main public discovery path works. | Event data changes often; assertions can become flaky if they expect exact event counts. | Allowed. Assert page structure and data shape, not exact inventory unless using a stable fixture. |
| Open event details | Review event description, time, place, rules, price, free places, organizer info, and sign-up options. | Verifies that public event details are readable before a user commits. | Event can sell out or be removed during the test. Details may contain user or organizer information. | Allowed for public fields only. Mask screenshots if participant names or profile data appear. |
| Register account | Create an account using email and phone, accept terms, and activate through email or SMS. | Important because account is required for many flows. | Creates real user data, may send email/SMS, may require parent/guardian data for minors, can pollute production. | Do not complete registration in live production unless the owner provides a disposable test identity and written approval. Form validation tests may stop before submit. |
| Login and logout | Sign in to an existing account, keep a session, and sign out. | Proves authenticated access still works. | Account lockout, credential leakage in logs, access to personal data. | Allowed only with a dedicated test account stored in environment variables or CI secrets. Never use a real personal account. |
| View account/profile | Read account settings, profile data, history, prepaid area, groups, or saved preferences. | Confirms authenticated pages load and authorization redirects work. | PII exposure in screenshots, reports, traces, and logs. | Allowed only for the dedicated test account. Mask PII in artifacts. Avoid participant lists unless they are public and expected. |
| Edit account/profile | Change profile fields, newsletter setting, password, benefit data, or linked social login. | Can prove validation and persistence. | Permanent account changes, broken test account, notification sends, security risk. | Conditional. Only reversible edits on a dedicated account, with cleanup in the same test. Password, phone, social login, and benefit-card changes are not live-safe. |
| Event sign-up | Join a sport event after reviewing details and terms. | This is the core business journey. | Real reservation, capacity change, payment/deposit obligation, confirmation notifications, operational impact for coordinator and other players. | Stop before final confirmation in production. Full sign-up belongs in staging, mocks, or a dedicated production test event approved by the owner. |
| Waitlist or reserve list | Join a list when an event is full. | Verifies full-event behavior. | Changes real queue order and can trigger notifications. | Stop before final submit. Do not join live waitlists. |
| Payment or prepaid top-up | Add funds, pay event fee, use bonus funds, or finish payment provider flow. | Proves revenue path works. | Real money movement, refunds, payment provider fraud signals, accounting noise. | Forbidden in live production. Use provider sandbox, mocks, or staging. |
| Deposit, surcharge, promo code, benefit partner | Use deposit money, partner card, surcharge, or discount code. | Covers important pricing rules. | Consumes real benefits or codes, changes deposit balance, creates financial or partner reconciliation issues. | Forbidden unless a production-safe test code/card and written approval exist. Prefer mocks or staging. |
| Cancel or resign from event | Leave an event and possibly receive refund or deposit penalty. | Tests a high-risk support and money path. | Can free a real place, charge penalties, trigger notifications, and change attendance planning. | Forbidden in live production automation. Model this in non-production. |
| Group flows | Create or join a group, invite users, split teams, or access group-only events. | Useful for social/team features. | Sends real invitations, exposes user data, changes access to private events. | No live production automation unless using isolated test-only group accounts and no real message delivery. |
| Organizer event publication | Create, edit, publish, or manage events as an organizer. | Validates supply-side flow. | Can publish real events, mislead users, trigger moderation or notifications. | Forbidden in public production by default. Use staging or a draft-only approved organizer account if available. |
| Coordinator or attendance flow | Manage attendance, lateness, no-shows, or participant status. | Important operational workflow. | Can affect deposits, late penalties, account blocking, attendance records, and real event quality. | Forbidden in live production automation. |
| Notifications | Receive email, SMS, push, newsletter, booking, cancellation, or status updates. | Verifies user communication. | Sends real messages and may create marketing or compliance noise. | Use test inboxes only. Do not trigger real SMS/push from production tests unless approved and rate-limited. |
| Static policy/help pages | Open terms, public rules, contact information, sport level explanations, or legal pages. | Confirms users can read required information. | Low risk, but links and PDFs can move. | Allowed as read-only link checks with low request volume. |
| Complaint/contact support | Send a complaint, support request, or contact form. | Verifies support path. | Creates real support tickets and wastes staff time. | Do not submit in live production. Only validate form fields before submit. |

## Risk Matrix

| Risk | Severity | Why it matters | Control |
|---|---:|---|---|
| Real booking or waitlist change | Critical | It changes capacity and can block a real user from joining an event. | Stop before final confirmation. Full flow only outside production or with an approved test event. |
| Real payment, prepaid top-up, deposit, refund, or benefit usage | Critical | It moves money or partner benefits and can affect accounting. | Never run against production payment rails. Use mocks, provider sandbox, or staging. |
| Notification side effects | High | Users, support, organizers, or coordinators can receive real email, SMS, or push messages. | Use test inboxes and avoid flows that send SMS/push. |
| Personal data exposure | High | Reports, traces, videos, logs, and screenshots can capture account or participant data. | Use synthetic accounts, mask PII, and restrict artifact retention. |
| Account lockout or account damage | High | Repeated login failures or profile edits can break the test account. | Keep credentials in secrets, avoid negative login loops on the same account, and limit profile writes. |
| Dynamic event data | Medium | Live events change by time, location, price, capacity, and status. | Assert stable shapes and ranges instead of exact event counts. |
| Time-sensitive rules | Medium | Cancellation, lateness, deposit, and no-show rules depend on event start time. | Do not automate those effects in production. Test the rules with controlled fixtures elsewhere. |
| Bot/rate-limit behavior | Medium | Frequent browser and API checks can be treated as abuse. | Keep smoke checks low-frequency, identify test traffic if possible, and avoid parallel scraping. |
| Third-party dependency outage | Medium | Payment, SMS, email, maps, social login, or benefit partners can fail independently. | Separate availability smoke from third-party contract tests. |
| Accessibility or visual false positives | Low | Dynamic content and consent banners can change screenshots. | Keep visual checks minimal and only for stable public surfaces. |

## Automation Boundaries

### Allowed In Live Production

- Load the public site in a browser.
- Check that the JavaScript app starts and key public routes render.
- Browse public event lists and event detail pages.
- Check public legal/static links, including terms PDFs.
- Log in and log out with a dedicated test account.
- Read the dedicated test account's own profile pages.
- Run low-frequency accessibility scans on public pages.
- Capture browser traces and screenshots only after masking or avoiding PII.

### Conditional In Live Production

These are allowed only with explicit approval and a dedicated test setup:

- Reversible profile edits on a test account.
- Newsletter preference changes on a test email.
- Final event sign-up only for an approved test event that cannot affect real users, money, waitlists, coordinator work, or notifications.
- Notification checks only with owned test inboxes and approved SMS/push limits.
- API checks only for read-only endpoints or test-account-owned data.

### Forbidden In Live Production By Default

- Completing event sign-up for real events.
- Joining or leaving waitlists.
- Cancelling reservations.
- Running prepaid top-up, payment, deposit, refund, bonus, promo, or benefit partner flows to completion.
- Creating, publishing, editing, or deleting organizer events.
- Managing attendance, lateness, no-shows, penalties, or coordinator state.
- Sending group invitations to real users.
- Submitting complaints or support/contact forms.
- Load testing, scraping, or high-parallel event inventory crawling.
- Using personal or employee accounts.
- Storing credentials, payment data, phone numbers, or PII in source control.

## Test Design Rules

1. Prefer read-only assertions.
   Example: verify that an event detail page shows a date, location, and visible call-to-action. Do not assert that exactly 12 events exist today.

2. Put a named stop point before every side effect.
   Example: a booking test may stop at `PreSubmitBookingSummary` and assert that the final button exists, then close the page.

3. Keep production data synthetic.
   Example: use `PNH_TEST_EMAIL`, `PNH_TEST_PASSWORD`, and `PNH_TEST_PHONE` from environment variables, never hard-coded values.

4. Make reports safe to share.
   Example: hide email, phone, profile name, participant names, tokens, cookies, and payment/session identifiers in traces and screenshots.

5. Separate smoke from regression.
   Example: production smoke should be small and read-only. Larger regression suites should run against staging or mocked services.

6. Treat time and inventory as unstable.
   Example: assert that event cards have valid dates and visible status text, not that a specific event is always available.

7. Fail closed around money and booking.
   Example: if a locator for the final confirmation button changes, the test should stop and fail instead of clicking a nearby button.

## Suggested Tags

Use tags or categories so production-safe commands can never accidentally run unsafe tests:

| Tag | Meaning |
|---|---|
| `Smoke` | Small, fast checks for public availability and login. |
| `ProductionSafe` | Explicitly approved for live production. |
| `ReadOnly` | Does not mutate server-side data. |
| `PreSubmit` | Walks a flow but stops before final submit. |
| `StagingOnly` | May mutate data, send messages, or complete business flows outside production. |
| `PaymentBoundary` | Exercises payment UI or contracts without real money movement. |
| `NoProduction` | Must never run against live production. |

## Example Production-Safe Scenarios

```gherkin
Scenario: Anonymous user can open the public event catalog
  Given I open the PilkaNaHali public site
  When the application has loaded
  Then I can see public event discovery content
  And every visible event card has a readable title, date, and location
```

```gherkin
Scenario: Test user reaches booking summary but does not reserve
  Given I am signed in with the dedicated production test account
  And I open a public event that allows sign-up
  When I follow the sign-up flow to the summary step
  Then I can see the final confirmation action
  And the test stops before clicking final confirmation
```

```gherkin
Scenario: Payment route is protected by a production boundary
  Given I am signed in with the dedicated production test account
  When I navigate to prepaid top-up
  Then I can see the available top-up entry point
  And the test does not submit a payment amount or open a real payment transaction
```

## Documentation And Command Notes

This change adds documentation only. It does not add new scripts or services.

When automation commands are added later, document each command beside the feature it runs. Production-safe examples should use an explicit production-safe `TestCaseFilter` in `config/test-run.runsettings`, for example:

```xml
<TestCaseFilter>Category=ProductionSafe</TestCaseFilter>
<TestCaseFilter>Category=Smoke&amp;Category=ReadOnly</TestCaseFilter>
<TestCaseFilter>Category=NoProduction</TestCaseFilter>
```

The `NoProduction` filter is shown only to make the boundary visible. It must be used against staging, local, or mocked environments, not against live production.
