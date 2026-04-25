# Branch Rules

## Goal

Branches should make work easy to review. A branch name should tell another person what kind of change it contains before they open the diff.

## Main Branch

`main` is the stable branch. It should contain work that builds and follows the current Definition of Done.

Do not commit experimental or half-finished work directly to `main` unless this is a solo local spike and no PR workflow is being used yet.

## Branch Naming

Use short names with this format:

```text
type/short-description
```

Recommended types:

| Type | Use for | Example |
|---|---|---|
| `docs` | Documentation-only changes. | `docs/project-vision` |
| `test` | New or changed tests. | `test/homepage-smoke` |
| `feat` | New framework or product-facing capability. | `feat/playwright-fixtures` |
| `fix` | Bug fixes. | `fix/login-wait` |
| `ci` | Pipeline or build automation. | `ci/github-actions-smoke` |
| `chore` | Maintenance that does not change behavior. | `chore/update-packages` |

Keep branch names lowercase and use hyphens between words.

## Commit Style

Use small commits that explain one idea at a time.

Recommended format:

```text
type: short summary
```

Examples:

```text
docs: add production automation map
test: add homepage smoke coverage
fix: wait for event cards before asserting
ci: upload playwright trace artifacts
```

## Pull Request Expectations

Every PR-style change should include:

- What changed.
- Why the change is needed.
- How it was verified.
- Any production-safety impact.
- Screenshots or report links when UI behavior changes.

For automation work, also state which category the tests belong to:

- `ProductionSafe`
- `ReadOnly`
- `PreSubmit`
- `StagingOnly`
- `NoProduction`

## Merge Rules

A change is ready to merge when:

- It builds locally or in CI.
- Relevant tests pass, or skipped tests are explained.
- Documentation is updated when commands, risks, categories, or behavior change.
- No credentials, tokens, cookies, traces with personal data, or payment data are committed.
- The Definition of Done has been checked.

## Production-Safety Review

Any change that touches live production automation must answer these questions:

1. Can this test create, update, or delete real production data?
2. Can this test reserve a real event place or change a waitlist?
3. Can this test move money, prepaid balance, deposit, bonus, promo code, or partner benefit state?
4. Can this test send email, SMS, push, support, or organizer messages?
5. Can this test expose personal data in screenshots, traces, logs, or reports?

If the answer is yes or unclear, mark the test `NoProduction` or `StagingOnly` until the risk is controlled.
