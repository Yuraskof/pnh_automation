using Microsoft.Playwright;

namespace PnhAutomation.Tests.Browser;

public sealed record BrowserViewport(string Name, int Width, int Height)
{
    public override string ToString() => Name;

    public ViewportSize ToViewportSize() =>
        new()
        {
            Width = Width,
            Height = Height
        };

    public RecordVideoSize ToRecordVideoSize() =>
        new()
        {
            Width = Width,
            Height = Height
        };
}
