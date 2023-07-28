public class AIPlugin
{
    public string SchemaVersion { get; set; } = "v1";

    public string NameForModel { get; set; } = "webscraper";

    public string NameForHuman { get; set; } = "webscraper";

    public string DescriptionForModel { get; set; } = "Scrapes the contents of a web URL";

    public string DescriptionForHuman { get; set; } = "Scrapes websites";

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