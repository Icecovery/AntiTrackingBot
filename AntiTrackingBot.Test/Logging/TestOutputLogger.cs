using Microsoft.Extensions.Logging;
using System;
using Xunit.Abstractions;

namespace AntiTrackingBot.Test.Logging;

internal class TestOutputLogger(ITestOutputHelper outputHelper) : ILogger
{
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	public bool IsEnabled(LogLevel logLevel) => true; // Enable all log levels

	public void Log<TState>(LogLevel logLevel,
		EventId eventId,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
	{
		// Write log message to the output helper
		outputHelper.WriteLine(formatter(state, exception));
	}
}