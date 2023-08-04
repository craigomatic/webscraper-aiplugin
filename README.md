# Webscraper AIplugin
AI Plugin that can be used to scrape useful information from a given URL. 

## Configuration

### Local dev
Open ```local.settings.json``` and add values for each of the following fields:

```CompletionConfig:AIService```

One of AzureOpenAI or OpenAI

```CompletionConfig:Endpoint``` 

Your Azure endpoint if using Azure OpenAI, or empty if using OpenAI

```CompletionConfig:DeploymentOrModelId``` 

Your Azure deployment name if using Azure OpenAI, or model name if using OpenAI

```CompletionConfig:Key``` 

Your API key

### Deploy to Azure
You'll need to have all the same values listed above entered into Configuration -> Application Settings

## Usage
Run it locally with this command:
```func run```

You can test functionality using the swagger UI, which will default to: http://localhost:7071/api/swagger/ui

Note that the first time the function runs, it will attempt to install the Playwright dependencies (chromium, etc) by running a powershell script and that requests to the endpoint will result in 503s while this is happening.

Once you've verified it locally using swagger, the next step is to try running it in the context of _other_ AI.

Some options I like for this are:

1. [Chat Copilot](https://github.com/microsoft/chat-copilot)
2. [sk-researcher](https://github.com/craigomatic/sk-researcher)
