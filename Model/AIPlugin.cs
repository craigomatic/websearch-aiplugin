public class AIPlugin
{
    public string SchemaVersion { get; set; } = "v1";

    public string NameForModel { get; set; } = "websearch";

    public string NameForHuman { get; set; } = "websearch";

    public string DescriptionForModel { get; set; } = "Searches the web";

    public string DescriptionForHuman { get; set; } = "Searches the web";

    public AIPluginAuth Auth { get; set; } = new AIPluginAuth { Type = "none" };

    public AIPluginAPI Api { get; set; } = new AIPluginAPI { Type = "openapi" };

    public string LogoUrl { get; set; } = string.Empty;

    public string LegalInfoUrl { get; set; } = string.Empty;
}

public class AIPluginAuth
{
    public string Type { get; set; } = string.Empty;
}

public class AIPluginAPI
{
    public string Type { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}