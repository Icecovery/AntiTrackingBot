using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace AntiTrackingBot.Test.Logging;
internal class TestLoggerProvider(IMessageSink diagnosticMessageSink) : ILoggerProvider
{
	public ILogger CreateLogger(string categoryName)
	{
		return new TestOutputLogger(diagnosticMessageSink);
	}

	public void Dispose() { }
}