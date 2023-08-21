# Websearch AIplugin
AI Plugin that can be used to search the internet

## Configuration

### Local dev
Open ```local.settings.json``` and add values for each of the following fields:

```AppConfig:BingApiKey```

Your Bing API key

### Deploy to Azure
You'll need to have all the same values listed above entered into Configuration -> Application Settings

## Usage
Run it locally with this command:

```func start```

You can test functionality using the swagger UI, which will default to: http://localhost:7298/api/swagger/ui

Once you've verified it locally using swagger, the next step is to try running it in the context of _other_ AI.

Some options I like for this are:

1. [Chat Copilot](https://github.com/microsoft/chat-copilot)
2. [sk-researcher](https://github.com/craigomatic/sk-researcher)
