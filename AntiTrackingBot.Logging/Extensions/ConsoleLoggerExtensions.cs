using Microsoft.Extensions.Logging;
using System;

namespace AntiTrackingBot.Logging.Extensions;
public static class ConsoleLoggerExtensions
{
	public static ILoggingBuilder AddBearConsole(this ILoggingBuilder builder, Action<BearConsoleFormatterOptions> configure)
	{
		builder.AddConsole(option =>
		{
			option.FormatterName = BearConsoleFormatter.FORMATTER_NAME;
		});

		builder.AddConsoleFormatter<BearConsoleFormatter, BearConsoleFormatterOptions>(configure);

		return builder;
	}
}
