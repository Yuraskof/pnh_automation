namespace PnhAutomation.Core.Configuration;

public sealed record AutomationSettings(Uri BaseUrl, string EnvironmentName)
{
    public const string BaseUrlVariableName = "PNH_BASE_URL";
    public const string EnvironmentNameVariableName = "PNH_ENVIRONMENT";
    public const string DefaultBaseUrl = "https://www.pilkanahali.pl/";
    public const string DefaultEnvironmentName = "Production";

    public static AutomationSettings ProductionDefaults() =>
        new(new Uri(DefaultBaseUrl), DefaultEnvironmentName);

    public static AutomationSettings FromProcessEnvironment() =>
        FromEnvironment(new Dictionary<string, string?>
        {
            [BaseUrlVariableName] = Environment.GetEnvironmentVariable(BaseUrlVariableName),
            [EnvironmentNameVariableName] = Environment.GetEnvironmentVariable(EnvironmentNameVariableName)
        });

    public static AutomationSettings FromEnvironment(
        IReadOnlyDictionary<string, string?> environmentVariables)
    {
        ArgumentNullException.ThrowIfNull(environmentVariables);

        var baseUrl = ReadValue(environmentVariables, BaseUrlVariableName) ?? DefaultBaseUrl;
        var environmentName = ReadValue(environmentVariables, EnvironmentNameVariableName) ?? DefaultEnvironmentName;

        return new AutomationSettings(CreateAbsoluteUri(baseUrl), environmentName);
    }

    private static string? ReadValue(
        IReadOnlyDictionary<string, string?> environmentVariables,
        string variableName)
    {
        return environmentVariables.TryGetValue(variableName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : null;
    }

    private static Uri CreateAbsoluteUri(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return uri;
        }

        throw new ArgumentException(
            $"Environment variable {BaseUrlVariableName} must be an absolute URL. Current value: '{value}'.",
            BaseUrlVariableName);
    }
}
