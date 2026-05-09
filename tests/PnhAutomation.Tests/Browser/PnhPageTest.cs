using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using PnhAutomation.Core.Configuration;

namespace PnhAutomation.Tests.Browser;

public abstract class PnhPageTest : PageTest
{
    private readonly string _artifactName;

    protected PnhPageTest()
    {
        Settings = AutomationSettings.FromProcessEnvironment();
        _artifactName = CreateArtifactName(GetType());
        ArtifactDirectory = Path.Combine(ResolveArtifactRoot(), GetType().Name, _artifactName);
    }

    protected AutomationSettings Settings { get; }

    protected string ArtifactDirectory { get; }

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions();
        var defaultViewport = BrowserViewports.DesktopSmall;

        options.BaseURL = Settings.BaseUrl.ToString();
        options.ViewportSize = defaultViewport.ToViewportSize();
        options.RecordVideoDir = Path.Combine(ArtifactDirectory, "videos");
        options.RecordVideoSize = defaultViewport.ToRecordVideoSize();

        return options;
    }

    public override async Task InitializeAsync()
    {
        Directory.CreateDirectory(ArtifactDirectory);

        await base.InitializeAsync().ConfigureAwait(false);

        await Context.Tracing.StartAsync(new TracingStartOptions
        {
            Title = $"{GetType().FullName}.{_artifactName}",
            Screenshots = true,
            Snapshots = true,
            Sources = true
        }).ConfigureAwait(false);
    }

    public override async Task DisposeAsync()
    {
        var failed = !TestOk;

        try
        {
            if (failed)
            {
                await CaptureFailureScreenshotAsync().ConfigureAwait(false);
            }

            await Context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = failed ? Path.Combine(ArtifactDirectory, "trace.zip") : null
            }).ConfigureAwait(false);
        }
        catch when (failed)
        {
            // Do not hide the original test failure with a secondary artifact failure.
        }
        finally
        {
            await base.DisposeAsync().ConfigureAwait(false);

            if (!failed)
            {
                DeletePassingTestArtifacts();
            }
        }
    }

    private async Task CaptureFailureScreenshotAsync()
    {
        Directory.CreateDirectory(ArtifactDirectory);

        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(ArtifactDirectory, "failure.png"),
            FullPage = true
        }).ConfigureAwait(false);
    }

    private void DeletePassingTestArtifacts()
    {
        if (!Directory.Exists(ArtifactDirectory))
        {
            return;
        }

        try
        {
            Directory.Delete(ArtifactDirectory, recursive: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Cleanup should not fail an otherwise passing browser test.
        }
    }

    private static string ResolveArtifactRoot()
    {
        var configuredArtifactRoot = Environment.GetEnvironmentVariable("PNH_ARTIFACT_DIR");
        if (!string.IsNullOrWhiteSpace(configuredArtifactRoot))
        {
            return Path.GetFullPath(configuredArtifactRoot.Trim());
        }

        return Path.Combine(ResolveRepositoryRoot(), "TestResults", "playwright");
    }

    private static string ResolveRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "pnh_automation.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Environment.CurrentDirectory;
    }

    private static string CreateArtifactName(Type testType)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
        var suffix = Guid.NewGuid().ToString("N")[..8];

        return $"{Sanitize(testType.Name)}-{timestamp}-{suffix}";
    }

    private static string Sanitize(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var safeCharacters = value.Select(character =>
            invalidCharacters.Contains(character) ? '-' : character);

        return new string(safeCharacters.ToArray());
    }
}
