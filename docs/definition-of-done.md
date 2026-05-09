# Definition of Done

## Purpose

The Definition of Done is the checklist used before a change is considered complete. It keeps the project consistent and prevents unsafe automation from reaching live production by accident.

In simple words: a change is not done just because the code was written. It is done when it is understandable, tested or intentionally documented, safe to run, and easy for the next person to maintain.

## General Checklist

A change is done when:

- The purpose of the change is clear from the README, docs, code, commit, or PR description.
- The implementation follows existing project patterns.
- The project builds.
- Relevant tests were run, or the reason they were not run is documented.
- New scripts or services have documented commands.
- Documentation is updated for changed behavior, commands, test categories, or safety boundaries.
- No unrelated files were reformatted or changed.
- No secrets, tokens, cookies, personal data, payment data, or private screenshots are committed.

## Documentation Changes

Documentation-only work is done when:

- The new page is linked from `README.md` or `docs/README.md`.
- The document uses simple language and concrete examples.
- Commands are included for any script or service mentioned.
- Live-production boundaries are stated when the topic touches production tests.
- The Markdown headings and links are easy to scan.

Example:

If a document mentions `install-playwright.ps1`, it must also show how to run it:

```powershell
pwsh ./scripts/install-playwright.ps1
```

If that script does not exist yet, the document must clearly say it is planned.

For the current Playwright package path, document the generated install command like this:

```powershell
dotnet build tests/PnhAutomation.Tests/PnhAutomation.Tests.csproj
pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 install
```

## Test Automation Changes

Automation code is done when:

- Tests have clear names that describe user behavior.
- Tests use stable locators and avoid timing sleeps unless there is no better option.
- Assertions check business meaning, not only technical implementation details.
- Test data comes from builders, fixtures, environment variables, or CI secrets.
- Production-safe tests are tagged explicitly.
- Unsafe tests are tagged `StagingOnly` or `NoProduction`.
- Browser traces, screenshots, videos, and logs do not expose secrets or personal data.

Example:

Good test name:

```text
AnonymousUser_CanOpenPublicEventDetails
```

Weak test name:

```text
Test1
```

## Production-Safety Checklist

Before a test can run against live production, confirm:

- It is read-only, uses only the dedicated test account, or stops before final submit.
- It cannot complete a booking, waitlist action, cancellation, payment, top-up, refund, deposit, benefit, promo, or organizer action.
- It cannot send real support, email, SMS, push, group, or organizer messages unless explicitly approved.
- It cannot change attendance, lateness, no-show, penalty, or coordinator state.
- It uses low request volume and does not scrape or load test production.
- It masks or avoids PII in artifacts.

If any item is unclear, the test is not production-safe.

## Code Review Checklist

A review should check:

- Correctness: does the change test or implement what it claims?
- Reliability: can it fail because of timing, live data changes, or flaky assumptions?
- Safety: can it mutate production or expose private data?
- Maintainability: will the next person understand how to extend it?
- Diagnostics: will a failure explain what broke?
- Documentation: are commands, boundaries, and changed behavior documented?

## Verification Commands

Run from the repository root:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet test --filter "Category=Unit"
```

For Docker:

```powershell
docker compose build
docker compose run --rm pnh_automation
```

`dotnet test` is the standard verification command. Browser-specific commands are documented in [Browser automation](browser-automation.md). Failed browser tests keep traces, screenshots, and videos under `TestResults/playwright` by default, and those artifacts must be reviewed for secrets or personal data before sharing.
