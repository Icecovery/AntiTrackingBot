namespace AntiTrackingBot.Core.Models;
public record CoreConfig
{
	/// <summary>
	/// List of tracking parameter filters
	/// </summary>
	public string[] Filters { get; set; } = [];

	/// <summary>
	/// Temporary path for storing downloaded files
	/// </summary>
	public string TemporaryPath { get; set; } = string.Empty;
}
