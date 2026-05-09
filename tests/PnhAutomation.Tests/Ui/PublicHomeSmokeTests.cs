using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Playwright;
using PnhAutomation.Core.Testing;
using PnhAutomation.Tests.Browser;

namespace PnhAutomation.Tests.Ui;

public sealed class PublicHomeSmokeTests : PnhPageTest
{
    private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";

    private static readonly string HeroHeadingXPath =
        HeadingContainingTextXPath("W co chcesz zagr");

    private static readonly string BlockedPageXPath =
        AnyElementContainingTextXPath("Sorry, you have been blocked");

    public static TheoryData<BrowserViewport> ResponsiveViewports
    {
        get
        {
            var data = new TheoryData<BrowserViewport>();

            foreach (var viewport in BrowserViewports.ResponsiveSmoke)
            {
                data.Add(viewport);
            }

            return data;
        }
    }

    [Fact]
    [Trait("Category", TestCategories.Ui)]
    [Trait("Category", TestCategories.Smoke)]
    [Trait("Category", TestCategories.ProductionSafe)]
    [Trait("Category", TestCategories.ReadOnly)]
    public async Task AnonymousUser_OpensPublicHomePage_AppLoads()
    {
        await OpenHomePageAsync();

        await Expect(Page).ToHaveTitleAsync(new Regex("^PNH$", RegexOptions.IgnoreCase));
        await Expect(Page.Locator(HeroHeadingXPath).First).ToBeVisibleAsync();
    }

    [Fact]
    [Trait("Category", TestCategories.Ui)]
    [Trait("Category", TestCategories.Smoke)]
    [Trait("Category", TestCategories.ProductionSafe)]
    [Trait("Category", TestCategories.ReadOnly)]
    public async Task AnonymousUser_ViewsPublicHomePage_PrimaryNavigationIsAvailable()
    {
        await OpenHomePageAsync();

        await AssertNavigationLinkAvailableAsync("ZNAJD");
        await AssertNavigationLinkAvailableAsync("DODAJ");
        await AssertNavigationLinkAvailableAsync("OFERTA");
        await AssertNavigationLinkAvailableAsync("JAK DZIA");
    }

    [Theory]
    [MemberData(nameof(ResponsiveViewports))]
    [Trait("Category", TestCategories.Ui)]
    [Trait("Category", TestCategories.Smoke)]
    [Trait("Category", TestCategories.ProductionSafe)]
    [Trait("Category", TestCategories.ReadOnly)]
    public async Task AnonymousUser_OpensPublicHomePage_LayoutFitsViewport(
        BrowserViewport viewport)
    {
        await Page.SetViewportSizeAsync(viewport.Width, viewport.Height);

        await OpenHomePageAsync();

        await Expect(Page.Locator(HeroHeadingXPath).First).ToBeVisibleAsync();

        var hasHorizontalOverflow = await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > window.innerWidth + 1");

        hasHorizontalOverflow.Should()
            .BeFalse($"the {viewport.Name} layout should fit the viewport without sideways scrolling");
    }

    private async Task OpenHomePageAsync()
    {
        var response = await Page.GotoAsync("/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded
        });

        response.Should().NotBeNull();
        response!.Ok.Should().BeTrue("the public home page should return a successful HTTP response");

        await Expect(Page.Locator(BlockedPageXPath))
            .Not.ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions
            {
                Timeout = 1_000
            });
    }

    private async Task AssertNavigationLinkAvailableAsync(string visibleText)
    {
        var link = Page.Locator(LinkByVisibleTextXPath(visibleText)).First;

        await Expect(link).ToBeVisibleAsync();

        var href = await link.GetAttributeAsync("href");

        href.Should().NotBeNullOrWhiteSpace();
        href.Should().NotStartWith("javascript", "navigation links should point to real routes");
    }

    private static string AnyElementContainingTextXPath(string text) =>
        $"//*[contains({NormalizedLowercaseTextXPath()}, {ToXPathLiteral(text.ToLowerInvariant())})]";

    private static string HeadingContainingTextXPath(string text) =>
        $"//*[self::h1 or self::h2][contains({NormalizedLowercaseTextXPath()}, {ToXPathLiteral(text.ToLowerInvariant())})]";

    private static string LinkByVisibleTextXPath(string text) =>
        $"//a[contains({NormalizedLowercaseTextXPath()}, {ToXPathLiteral(text.ToLowerInvariant())})]";

    private static string NormalizedLowercaseTextXPath() =>
        $"translate(normalize-space(.), '{UppercaseLetters}', '{LowercaseLetters}')";

    private static string ToXPathLiteral(string value)
    {
        if (!value.Contains('\''))
        {
            return $"'{value}'";
        }

        if (!value.Contains('"'))
        {
            return $"\"{value}\"";
        }

        var parts = value.Split('\'')
            .Select(part => $"'{part}'");

        return $"concat({string.Join(", \"'\", ", parts)})";
    }
}
