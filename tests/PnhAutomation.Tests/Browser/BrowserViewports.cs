namespace PnhAutomation.Tests.Browser;

public static class BrowserViewports
{
    public static readonly BrowserViewport DesktopSmall = new("desktop", 1280, 720);

    public static readonly BrowserViewport MobilePhone = new("mobile", 390, 844);

    public static IReadOnlyList<BrowserViewport> ResponsiveSmoke { get; } =
    [
        DesktopSmall,
        MobilePhone
    ];
}
