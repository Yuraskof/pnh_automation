# Test Strategy

Last reviewed: 2026-04-25

This document defines the first testing strategy for the PNH Automation project. It explains the test pyramid, the difference between smoke and regression tests, the risk matrix, and the naming standard.

In simple words: the project should not test everything through the browser. Browser tests are useful, but they are slower and more fragile than lower-level checks. The strongest strategy is to put many checks near the code or API, then keep only the most important user journeys as browser tests.

## Test Pyramid

The test pyramid shows how many tests should exist at each level. The lower levels should have more tests because they are faster, cheaper, and easier to diagnose. The top level should have fewer tests because full browser journeys are slower and can fail because of live data, network timing, or third-party services.

```text
                Few
        +----------------+
        | End-to-end UI  |
        +----------------+
        | API/contract   |
        +----------------+
        | Unit/component |
        +----------------+
        | Static checks  |
        +----------------+
               Many
```

## Pyramid Levels

| Level | Purpose | Example | Target size | Production use |
|---|---|---|---:|---|
| Static checks | Catch simple problems before runtime. | Build, formatting, nullable warnings, secret scanning. | Many | Safe. |
| Unit/component tests | Check small rules without browser or network. | Price formatting, date parsing, config mapping, test data builders. | Many | Run locally and in CI. |
| API/contract tests | Check service responses, schema, status codes, and auth behavior. | Public events endpoint returns valid date, status, location fields. | Medium | Read-only only in production. Wider coverage in staging. |
| End-to-end UI tests | Check important user behavior in a real browser. | Anonymous user opens event details; test user logs in and logs out. | Few | Only `ProductionSafe`, `ReadOnly`, or `PreSubmit` tests. |
| Visual/accessibility/performance smoke | Check selected quality signals, not every screen. | Axe scan on home page; page load budget for public event page. | Few | Low-frequency and only on stable public surfaces. |

## Smoke And Regression Split

Smoke tests answer: "Is the most important path alive right now?"

Regression tests answer: "Did a change break expected behavior across the product?"

| Suite | Goal | Typical runtime | Environment | Includes | Excludes |
|---|---|---:|---|---|---|
| `Smoke` | Fast confidence that the app is available and core paths work. | 1-5 minutes | Production-safe, staging, or local depending on tags. | Public app load, public event discovery, event details, login/logout with test account. | Payment completion, real booking, real cancellation, notification sending, organizer changes. |
| `Regression` | Broader confidence before release or scheduled nightly runs. | 10-45 minutes at first, can grow later. | Staging or controlled test environment by default. | Browser journeys, API contracts, validation, negative cases, accessibility, selected visual checks. | Live production side effects unless explicitly marked safe. |
| `ProductionSafe` | Small subset approved for live production. | 1-5 minutes | Live production. | Read-only public checks, dedicated test account login/logout, pre-submit checks. | Anything that mutates real data or moves money. |
| `NoProduction` | Tests that must not run against live production. | Any | Local, mocks, or staging. | Full booking, cancellation, payment, prepaid, organizer, notifications. | Live production execution. |

## Suite Selection

Choose the active suite by editing `RunConfiguration/TestCaseFilter` in `config/test-run.runsettings`, then run:

```powershell
dotnet test
```

Common filter values:

```xml
<TestCaseFilter>Category=Smoke</TestCaseFilter>
<TestCaseFilter>Category=Regression</TestCaseFilter>
<TestCaseFilter>Category=ProductionSafe</TestCaseFilter>
<TestCaseFilter>Category=Smoke&amp;Category=ReadOnly</TestCaseFilter>
<TestCaseFilter>Category=NoProduction</TestCaseFilter>
```

The `NoProduction` filter is documented to make the boundary visible. It must be pointed at local, mocked, or staging environments, not live production.

There are no new scripts or services in this documentation change. When scripts are added later, their run commands must be added to the README or a dedicated commands document.

## Risk Matrix

Use this matrix to decide what to automate first and where it should run.

| Area | User/business risk | Automation priority | Recommended level | Live production boundary |
|---|---|---:|---|---|
| Public app availability | Users cannot open the service. | High | Smoke UI + lightweight static link check. | Allowed as low-frequency `ProductionSafe`. |
| Event browsing | Users cannot find relevant sport events. | High | UI smoke, API contract, regression UI. | Allowed when read-only. Avoid exact event counts. |
| Event details | Users cannot see time, place, price, status, or rules before joining. | High | UI smoke + API contract. | Allowed for public fields. Mask PII if visible. |
| Login/logout | Users cannot access account-only features. | High | Smoke UI + auth API checks where available. | Allowed only with dedicated test account and secrets. |
| Registration | New users cannot create an account. | Medium | UI validation + staging E2E. | Stop before submit in production unless approved. |
| Profile/account | Users cannot view or update their data. | Medium | Authenticated UI + API checks. | Read-only on dedicated account. Reversible edits only with approval. |
| Event sign-up | Users cannot join events; business loses bookings. | Critical | Staging E2E + API/contract + pre-submit production check. | Stop before final confirmation in production. |
| Waitlist | Full-event users cannot join reserve list. | High | Staging E2E + validation checks. | Do not join live waitlists. |
| Payment/prepaid/deposit | Money or balances can be wrong. | Critical | Provider sandbox, mocks, API contracts, staging E2E. | Forbidden in live production by default. |
| Cancellation/resignation | Users may lose places, refunds, or deposits incorrectly. | Critical | Staging E2E + unit/API rule tests. | Forbidden in live production by default. |
| Notifications | Users miss or receive wrong email, SMS, or push messages. | High | Test inboxes, mocked provider checks, staging E2E. | Do not trigger real SMS/push unless approved. |
| Organizer/coordinator actions | Real events or attendance can be changed incorrectly. | Critical | Staging-only UI/API tests. | Forbidden in public production by default. |
| Legal/static pages | Users cannot read required policies or help content. | Medium | Link checks + UI smoke. | Allowed as read-only. |
| Accessibility | Users with assistive technology are blocked. | Medium | Axe scan on stable public pages. | Allowed at low frequency. |

## Automation Selection Rules

Use the lowest reliable level for each check:

1. If a rule can be tested without a browser, prefer unit or component tests.
   Example: a date display helper should be tested directly, not by opening five browser pages.

2. If a response contract matters, prefer API tests.
   Example: event detail data should have a valid date, location, price/status field, and event id.

3. If the question is "can a user complete this visible journey?", use a UI test.
   Example: a public user opens the site, browses events, and opens event details.

4. If a journey mutates data, run it outside production.
   Example: confirming event sign-up belongs in staging or a mocked environment.

5. If a production UI test reaches a dangerous button, assert the pre-submit state and stop.
   Example: check that the final booking confirmation button is visible, then close the page.

## Test Categories

Use categories consistently so commands are safe:

| Category | Meaning |
|---|---|
| `Unit` | Fast checks for shared core rules and helper behavior. |
| `Smoke` | Fast check for core availability or a critical happy path. |
| `Regression` | Broader test for release confidence. |
| `ProductionSafe` | Approved for live production. |
| `ReadOnly` | Does not mutate server-side data. |
| `PreSubmit` | Walks to a final action but does not submit it. |
| `Api` | Tests HTTP/API behavior. |
| `Ui` | Tests browser behavior. |
| `Accessibility` | Runs accessibility checks. |
| `Visual` | Runs screenshot or visual comparison checks. |
| `StagingOnly` | May mutate data or use controlled test services. |
| `NoProduction` | Must never run against live production. |

Example category combinations:

```text
Smoke + Ui + ProductionSafe + ReadOnly
Regression + Api + StagingOnly
Regression + Ui + NoProduction
Smoke + Ui + ProductionSafe + PreSubmit
```

## Test Naming Standard

Test names should describe user behavior and expected result. A reviewer should understand the intent without opening the test body.

Use this pattern:

```text
Actor_Action_ExpectedResult
```

Good examples:

```text
AnonymousUser_OpensPublicHomePage_AppLoads
AnonymousUser_OpensEventDetails_EventCoreFieldsAreVisible
TestUser_LogsIn_AccountMenuIsVisible
TestUser_StartsEventSignup_StopsAtPreSubmitSummary
PaymentBoundary_OpensTopUpPage_DoesNotCreateTransaction
```

Weak examples:

```text
Test1
HomePageWorks
Login
ShouldPass
ClickButton
```

## Naming Rules

- Use PascalCase words separated by underscores.
- Start with the actor or system area, such as `AnonymousUser`, `TestUser`, `EventCatalog`, `PaymentBoundary`, or `Api`.
- Include the action, such as `OpensEventDetails`, `LogsIn`, `StartsEventSignup`, or `RequestsPublicEvents`.
- End with the expected result, such as `AppLoads`, `EventCoreFieldsAreVisible`, or `StopsAtPreSubmitSummary`.
- Avoid implementation details in the name.
- Avoid vague words like `Works`, `Success`, or `Check`.
- If the test protects a production boundary, make that visible in the name.

Boundary example:

```text
TestUser_StartsEventSignup_StopsBeforeFinalConfirmation
```

This is better than:

```text
TestUser_SignupWorks
```

The first name clearly says the test does not complete a real booking.

## Folder And Class Naming

Planned structure when the test project is added:

```text
tests/
  PnhAutomation.Tests/
    Smoke/
      PublicHomeSmokeTests.cs
      EventCatalogSmokeTests.cs
    Regression/
      EventDetailsRegressionTests.cs
      LoginRegressionTests.cs
    Api/
      PublicEventsApiTests.cs
    Accessibility/
      PublicPagesAccessibilityTests.cs
```

Class names should group related behavior:

```text
PublicHomeSmokeTests
EventCatalogSmokeTests
LoginRegressionTests
PaymentBoundaryTests
```

Method names should describe one scenario:

```text
AnonymousUser_OpensPublicHomePage_AppLoads
AnonymousUser_FiltersPublicEvents_VisibleResultsRemainValid
TestUser_LogsOut_LoginEntryPointReturns
```

## First Test Priorities

Build coverage in this order:

1. Public home page smoke. Implemented in `PublicHomeSmokeTests` with homepage load, primary navigation availability, and desktop/mobile viewport-fit checks.
2. Public event catalog smoke.
3. Event detail smoke.
4. Dedicated test account login/logout smoke.
5. Pre-submit event sign-up boundary.
6. Read-only public API contract checks if stable endpoints are discovered.
7. Registration and profile validation in staging.
8. Payment, prepaid, cancellation, waitlist, organizer, and notification flows in staging or mocks only.

This order gives useful confidence early while keeping live production safe.
