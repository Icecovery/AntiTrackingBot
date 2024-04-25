# Anti-Tracking Bot

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Icecovery/AntiTrackingBot/dotnet.yml?branch=master&style=flat-square)
![GitHub License](https://img.shields.io/github/license/Icecovery/AntiTrackingBot?style=flat-square)


A simple bot that will detect and clean tracking links from messages.

Currently, the bot is implemented for Telegram, but it can be easily adapted to other platforms.

## Example Configuration

Create a file named `config.jsonc` in the same directory as the bot executable:

```json
{
	"CoreConfig":
	{
		"Filters": [
			"https://raw.githubusercontent.com/AdguardTeam/AdguardFilters/master/TrackParamFilter/sections/specific.txt",
			"https://raw.githubusercontent.com/AdguardTeam/AdguardFilters/master/TrackParamFilter/sections/general_url.txt"
		],
		"TemporaryPath": "tmp/"
	},
	"BotConfig":
	{
		"TelegramKey": "YOUR_TELEGRAM_BOT_TOKEN", // Get it from @BotFather
		"BotName": "YOUR_TELEGRAM_BOT_NAME", // Without @
		"UpdatePeriodHours": 6, // How often to check for updates
		"ProjectLinkUrl": "https://github.com/Icecovery/AntiTrackingBot" // The link to the project
	}
}
```

## Attribution

Without the work of the Adguard team, this bot would not be possible: [Adguard Filters](https://github.com/AdguardTeam/AdguardFilters)

[URL Detector](https://github.com/eladaus/URL-Detector) by [eladaus](https://github.com/eladaus) (Apache License 2.0)

[Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) by [TelegramBots](https://github.com/TelegramBots) (MIT License)


