# Webscraper AIplugin
AI Plugin that can be used to scrape useful information from a given URL. 

## Usage
You can run it locally and test functionality using the swagger UI, which will default to: http://localhost:7071/api/swagger/ui

Note that the first time the function runs, it will attempt to install the Playwright dependencies (chromium, etc) by running a powershell script and that requests to the endpoint will result in 503s while this is happening.

Once you've verified it locally using swagger, the next step is to try running it in the context of _other_ AI.

Some options I like for this are:

1. [Chat Copilot](https://github.com/microsoft/chat-copilot)
2. [sk-researcher](https://github.com/craigomatic/sk-researcher)

Note: Currently there is an issue getting the function to run the Powershell script when published to Azure.