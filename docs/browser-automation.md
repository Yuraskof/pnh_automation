# Browser Automation

## Purpose

Browser tests use Playwright to open the real site in a real browser. In simple words: unit tests check small pieces of code, while browser tests check what a user can see and do on the page.

Because the default target is live production, browser tests must stay low-volume and production-safe. They should read public information, use a dedicated test account when authentication is needed, and stop before booking, payment, prepaid, notification, or organizer side effects.

## Install Browsers

Run these commands from the repository root after restoring packages:

```powershell
dotnet restore
dotnet build tests/PnhAutomation.Tests/PnhAutomation.Tests.csproj
pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 install
```

On a Linux CI runner or a fresh Linux machine, install Playwright system dependencies too:

```powershell
dotnet build tests/PnhAutomation.Tests/PnhAutomation.Tests.csproj
pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 install --with-deps
```

The `dotnet build` step matters because Playwright creates `playwright.ps1` inside the test project's `bin` folder during build.

## Base URL Config

Browser tests inherit the same base URL config as the rest of the framework:

| Environment variable | Default | Purpose |
|---|---|---|
| `PNH_BASE_URL` | `https://www.pilkanahali.pl/` | Site opened by Playwright when a test uses relative paths like `/`. |
| `PNH_ENVIRONMENT` | `Production` | Human-readable environment name for reports and diagnostics. |
| `PNH_ARTIFACT_DIR` | `TestResults/playwright` | Optional folder for failed browser traces, screenshots, and videos. |

Example:

```powershell
$env:PNH_BASE_URL = "https://www.pilkanahali.pl/"
$env:PNH_ENVIRONMENT = "Production"
$env:PNH_ARTIFACT_DIR = "D:\test-artifacts\pnh"
dotnet test --filter "Category=Ui"
```

If `PNH_BASE_URL` is changed to a staging site, make sure the test categories match the target. Tests that can change data must not run against production.

## Run Browser Tests

Run all UI tests:

```powershell
dotnet test --filter "Category=Ui"
```

Run only production-safe UI smoke tests:

```powershell
dotnet test --filter "Category=Ui&Category=Smoke&Category=ProductionSafe"
```

Run in a visible browser window:

```powershell
$env:HEADED = "1"
dotnet test --filter "Category=Ui"
```

Run with a specific browser:

```powershell
$env:BROWSER = "webkit"
dotnet test --filter "Category=Ui"
```

Debug with the Playwright inspector:

```powershell
$env:PWDEBUG = "1"
dotnet test --filter "Category=Ui"
```

## Test Base Class

New browser tests should inherit `PnhPageTest`. The base class creates a Playwright context with:

- `BaseURL` set from `PNH_BASE_URL`.
- A stable `1280x720` viewport.
- Trace recording with screenshots, snapshots, and source files.
- A full-page `failure.png` screenshot when the test fails.
- Video recording during the test, kept only when the test fails.

Example:

```csharp
using System.Text.RegularExpressions;
using PnhAutomation.Core.Testing;
using PnhAutomation.Tests.Browser;

namespace PnhAutomation.Tests.Ui;

public sealed class HomePageSmokeTests : PnhPageTest
{
    [Fact]
    [Trait("Category", TestCategories.Ui)]
    [Trait("Category", TestCategories.Smoke)]
    [Trait("Category", TestCategories.ProductionSafe)]
    [Trait("Category", TestCategories.ReadOnly)]
    public async Task AnonymousUser_CanOpenHomePage()
    {
        await Page.GotoAsync("/");

        await Expect(Page).ToHaveTitleAsync(new Regex("Hali", RegexOptions.IgnoreCase));
    }
}
```

## Failure Artifacts

Failed browser tests keep artifacts under:

```text
TestResults/playwright/<TestClass>/<RunId>/
```

The folder contains:

| File | Meaning |
|---|---|
| `trace.zip` | Step-by-step Playwright trace with DOM snapshots and action screenshots. |
| `failure.png` | Full-page screenshot captured during test cleanup. |
| `videos/*.webm` | Browser video kept only for failed tests. |

Open a trace locally:

```powershell
pwsh ./tests/PnhAutomation.Tests/bin/Debug/net10.0/playwright.ps1 show-trace ./TestResults/playwright/<TestClass>/<RunId>/trace.zip
```

Do not commit `TestResults`. Traces, screenshots, and videos can contain page content, cookies, tokens, names, emails, or other personal data.
