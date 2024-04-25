using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace AntiTrackingBot.Test.Logging;
internal class TestLoggerProvider(ITestOutputHelper outputHelper) : ILoggerProvider
{
	public ILogger CreateLogger(string categoryName)
	{
		return new TestOutputLogger(outputHelper);
	}

	public void Dispose() { }
}