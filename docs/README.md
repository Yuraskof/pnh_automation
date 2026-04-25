# Documentation Index

This folder contains the working documentation for the PNH Automation project. It is part of the deliverable, not a side note. A senior automation project should make its intent, risks, and operating rules visible before the test suite grows.

## Reading Order

1. [Project vision](project-vision.md)
   Start here to understand what the project is trying to prove and what is out of scope.

2. [Public production automation map](public-production-automation-map.md)
   Use this before designing any test that touches the live PilkaNaHali.pl site.

3. [Framework architecture](framework-architecture.md)
   Use this to understand the solution layout, shared core, test project, and Docker test runner.

4. [Test strategy](test-strategy.md)
   Use this to choose the right test level, suite, risk priority, and test name.

5. [Branch rules](branch-rules.md)
   Use this before starting a new change so branch names, commits, and reviews stay consistent.

6. [Definition of Done](definition-of-done.md)
   Use this before opening or finishing a PR-style change.

## Documentation Rules

- Keep documents short enough to be useful during real work.
- Add simple examples when a rule can be misunderstood.
- Document every new script or service with commands showing how to run it.
- Update existing docs in the same change when behavior, commands, categories, or safety rules change.
- Prefer one focused page per topic instead of one very large document.

## Planned Docs

These documents should be added when the related implementation work starts:

| Document | Purpose |
|---|---|
| `commands.md` | Complete local, CI, Docker, and report commands once scripts exist. |
| `troubleshooting.md` | Common failures, diagnostics, and known production-safe stop points. |
