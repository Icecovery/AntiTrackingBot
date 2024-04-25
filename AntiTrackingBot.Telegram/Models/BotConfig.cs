namespace AntiTrackingBot.Telegram.Models;
public record BotConfig
{
	/// <summary>
	/// The Telegram API key for the bot.
	/// </summary>
	public string TelegramKey { get; set; } = string.Empty;

	/// <summary>
	/// The name of the bot
	/// </summary>
	public string BotName { get; set; } = string.Empty;

	/// <summary>
	/// The hours between each update of the filters.
	/// </summary>
	public int UpdatePeriodHours { get; set; } = 6;

	public string ProjectLinkUrl { get; set; } = string.Empty;

}
