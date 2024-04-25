using AntiTrackingBot.Logging.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;

namespace AntiTrackingBot.Logging;
public sealed class BearConsoleFormatterOptions : ConsoleFormatterOptions
{
	public bool UseUTCTimestamp { get; set; } = false;

	public ConsoleColors NormalColor { get; set; } = new ConsoleColors(ConsoleColor.Gray);
	public ConsoleColors TraceColor { get; set; } = new ConsoleColors(ConsoleColor.Green);
	public ConsoleColors DebugColor { get; set; } = new ConsoleColors(ConsoleColor.White);
	public ConsoleColors InfoColor { get; set; } = new ConsoleColors(ConsoleColor.Cyan);
	public ConsoleColors WarnColor { get; set; } = new ConsoleColors(ConsoleColor.Yellow);
	public ConsoleColors ErrorColor { get; set; } = new ConsoleColors(ConsoleColor.Black, ConsoleColor.DarkRed);
	public ConsoleColors CriticalColor { get; set; } = new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed);

	public ConsoleColors GetLogLevelColors(LogLevel logLevel)
	{
		return logLevel switch
		{
			LogLevel.Trace => TraceColor,
			LogLevel.Debug => DebugColor,
			LogLevel.Information => InfoColor,
			LogLevel.Warning => WarnColor,
			LogLevel.Error => ErrorColor,
			LogLevel.Critical => CriticalColor,
			_ => new ConsoleColors(),
		};
	}
}
