# PNH Automation

Automation portfolio project for [PilkaNaHali.pl](https://www.pilkanahali.pl/).

The suite should explain what is tested, why it matters, what is safe to run in live production, and how the same checks can be promoted into CI with readable reports.

## Current State

This repository currently contains a .NET shared core library, an xUnit test project, Playwright browser test wiring, Docker test runner wiring, and documentation for the planned automation framework. The production automation boundaries are documented before live browser coverage grows, so future code has clear safety rules from the start.

## Repository Structure

```text
pnh_automation/
  src/PnhAutomation.Core/         shared automation core library
  tests/PnhAutomation.Tests/      xUnit test project
  docs/                           project documentation and test strategy
  Dockerfile                      containerized test runner
  compose.yaml                    Docker Compose service definition
  PLAN.md                         implementation roadmap notes
  README.md                       project entry point
```

## Project Rules

- Production tests must be read-only, use a dedicated test account, or stop before a real side effect.
- Booking, payment, prepaid, cancellation, waitlist, notification, and organizer flows must not be completed against live production by default.
- Credentials must come from environment variables or CI secrets.
- Every new script or service must be documented with commands showing how to run it.
- Each meaningful change should include either tests, documentation, or a clear explanation of why neither applies.

## Documentation

- [Docs index](docs/README.md): documentation map and recommended reading order.
- [Project vision](docs/project-vision.md): goals, scope, audience, and success criteria.
- [Framework architecture](docs/framework-architecture.md): solution layout, project responsibilities, and run commands.
- [Browser automation](docs/browser-automation.md): Playwright install/run commands, base URL config, and failure artifacts.
- [Test strategy](docs/test-strategy.md): test pyramid, smoke/regression split, risk matrix, and naming standard.
- [Branch rules](docs/branch-rules.md): branch names, commit style, PR expectations, and merge rules.
- [Definition of Done](docs/definition-of-done.md): checklist for code, tests, docs, safety, and review readiness.
- [Public production automation map](docs/public-production-automation-map.md): public user journeys, live-production risks, and automation boundaries.

## Commands

Run from the repository root:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet test --filter "Category=Unit"
dotnet build tests/PnhAutomation.Tests/PnhAutomation.Tests.csproj
pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 install
dotnet test --filter "Category=Ui"
docker compose build
docker compose run --rm pnh_automation
```

`docker compose run --rm pnh_automation` uses the repository Dockerfile and runs `dotnet test pnh_automation.sln --no-restore` inside the SDK container.

Browser tests use `PNH_BASE_URL`, which defaults to `https://www.pilkanahali.pl/`:

```powershell
$env:PNH_BASE_URL = "https://www.pilkanahali.pl/"
$env:BROWSER = "chromium"
dotnet test --filter "Category=Ui&Category=ProductionSafe"
```

Failed browser tests keep Playwright traces, screenshots, and videos under `TestResults/playwright` by default.

Future production-safe test commands should use explicit filters, for example:

```powershell
dotnet test --filter "Category=ProductionSafe"
dotnet test --filter "Category=Smoke&Category=ReadOnly"
dotnet test --filter "Category=Regression"
```
