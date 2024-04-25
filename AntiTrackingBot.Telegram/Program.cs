using AntiTrackingBot.Core;
using AntiTrackingBot.Logging.Extensions;
using AntiTrackingBot.Telegram.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AntiTrackingBot.Telegram;

public class Program
{
	private static readonly CancellationTokenSource Cts = new();

	public static async Task Main(string[] args)
	{
		IConfiguration config = new ConfigurationBuilder()
			.AddJsonFile("config.jsonc", optional: false, reloadOnChange: true)
			.AddEnvironmentVariables()
			.Build();

		await Host.CreateDefaultBuilder(args)
			.ConfigureLogging(builder =>
			{
				builder.AddBearConsole(options =>
				{
					options.UseUtcTimestamp = false;
					options.IncludeScopes = true;
					options.TimestampFormat = "s";
				});

#if DEBUG
				builder.SetMinimumLevel(LogLevel.Trace);
#else
				builder.SetMinimumLevel(LogLevel.Information);
#endif
			})
			.ConfigureServices(services =>
			{
				services.AddSingleton(config);
				services.AddSingleton<IAntiTrackingCore, AntiTrackingCore>();
				services.AddHostedService<AntiTrackingUpdaterService>();
				services.AddHostedService<BotService>();
			})
			.Build()
			.RunAsync(Cts.Token);
	}
}
