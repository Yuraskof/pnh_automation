# PNH Automation

Automation portfolio project for [PilkaNaHali.pl](https://www.pilkanahali.pl/).

The suite should explain what is tested, why it matters, what is safe to run in live production, and how the same checks can be promoted into CI with readable reports.

## Current State

This repository currently contains a .NET shared core library, an xUnit test project, Docker test runner wiring, and documentation for the planned automation framework. The production automation boundaries are documented before live browser tests are added, so future code has clear safety rules from the start.

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
docker compose build
docker compose run --rm pnh_automation
```

`docker compose run --rm pnh_automation` uses the repository Dockerfile and runs `dotnet test pnh_automation.sln --no-restore` inside the SDK container.

Future production-safe test commands should use explicit filters, for example:

```powershell
dotnet test --filter "Category=ProductionSafe"
dotnet test --filter "Category=Smoke&Category=ReadOnly"
dotnet test --filter "Category=Regression"
```
