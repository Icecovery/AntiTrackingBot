using AntiTrackingBot.Logging.Extensions;
using AntiTrackingBot.Logging.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace AntiTrackingBot.Logging;
public class BearConsoleFormatter : ConsoleFormatter, IDisposable
{
	public const string FORMATTER_NAME = "bear";

	private readonly IDisposable? _optionsReloadToken;
	private BearConsoleFormatterOptions _formatterOptions;

	private void ReloadLoggerOptions(BearConsoleFormatterOptions options) => _formatterOptions = options;

	public BearConsoleFormatter(IOptionsMonitor<BearConsoleFormatterOptions> options) : base(FORMATTER_NAME)
	{
		_optionsReloadToken = options.OnChange(ReloadLoggerOptions);
		_formatterOptions = options.CurrentValue;
	}


	public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
	{
		string? message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

		if (message is null)
		{
			return;
		}

		DateTimeOffset dateTimeOffset = _formatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
		string timestamp = dateTimeOffset.ToString(_formatterOptions.TimestampFormat ?? "HH:mm:ss");

		ConsoleColors consoleColors = _formatterOptions.GetLogLevelColors(logEntry.LogLevel);

		textWriter.WriteColoredMessage($"[{timestamp}]", consoleColors);
		textWriter.Write(" ");

		if (_formatterOptions.IncludeScopes)
		{
			textWriter.WriteColoredMessage($"{logEntry.Category}[{logEntry.EventId.Id}]", consoleColors);
			textWriter.WriteLine();
			textWriter.Write("    ");
		}

		textWriter.WriteColoredMessage(message, _formatterOptions.NormalColor);
		textWriter.WriteLine();
	}

	public void Dispose()
	{
		_optionsReloadToken?.Dispose();
	}
}
