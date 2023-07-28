public class ModelConfig
{
    public const string OpenAI = "OPENAI";
    public const string AzureOpenAI = "AZUREOPENAI";

    public string AIService { get; set; } = string.Empty;
    public string DeploymentOrModelId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;

    public bool IsValid()
    {
        switch (this.AIService.ToUpperInvariant())
        {
            case OpenAI:
                return
                    !string.IsNullOrEmpty(this.DeploymentOrModelId) &&
                    !string.IsNullOrEmpty(this.Key);

            case AzureOpenAI:
                return
                    !string.IsNullOrEmpty(this.Endpoint) &&
                    !string.IsNullOrEmpty(this.DeploymentOrModelId) &&
                    !string.IsNullOrEmpty(this.Key);
        }

        return false;
    }
}