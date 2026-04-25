using FluentAssertions;
using PnhAutomation.Core.Configuration;
using PnhAutomation.Core.Testing;

namespace PnhAutomation.Tests.Core.Configuration;

public sealed class AutomationSettingsTests
{
    [Fact]
    [Trait("Category", TestCategories.Unit)]
    public void AutomationSettings_CreatesProductionDefaults_UsesPublicBaseUrl()
    {
        var settings = AutomationSettings.ProductionDefaults();

        settings.BaseUrl.Should().Be(new Uri("https://www.pilkanahali.pl/"));
        settings.EnvironmentName.Should().Be("Production");
    }

    [Fact]
    [Trait("Category", TestCategories.Unit)]
    public void AutomationSettings_ReadsEnvironmentValues_UsesConfiguredBaseUrlAndName()
    {
        var environmentVariables = new Dictionary<string, string?>
        {
            [AutomationSettings.BaseUrlVariableName] = "https://example.test/",
            [AutomationSettings.EnvironmentNameVariableName] = "Staging"
        };

        var settings = AutomationSettings.FromEnvironment(environmentVariables);

        settings.BaseUrl.Should().Be(new Uri("https://example.test/"));
        settings.EnvironmentName.Should().Be("Staging");
    }

    [Fact]
    [Trait("Category", TestCategories.Unit)]
    public void AutomationSettings_ReadsBlankEnvironmentValues_UsesProductionDefaults()
    {
        var environmentVariables = new Dictionary<string, string?>
        {
            [AutomationSettings.BaseUrlVariableName] = " ",
            [AutomationSettings.EnvironmentNameVariableName] = null
        };

        var settings = AutomationSettings.FromEnvironment(environmentVariables);

        settings.Should().Be(AutomationSettings.ProductionDefaults());
    }

    [Fact]
    [Trait("Category", TestCategories.Unit)]
    public void AutomationSettings_ReadsInvalidBaseUrl_ThrowsHelpfulError()
    {
        var environmentVariables = new Dictionary<string, string?>
        {
            [AutomationSettings.BaseUrlVariableName] = "not-a-url"
        };

        var action = () => AutomationSettings.FromEnvironment(environmentVariables);

        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("*PNH_BASE_URL*absolute URL*");
    }
}
