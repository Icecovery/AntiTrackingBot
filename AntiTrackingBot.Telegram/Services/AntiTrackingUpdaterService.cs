using AntiTrackingBot.Core;
using AntiTrackingBot.Telegram.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AntiTrackingBot.Telegram.Services;
internal class AntiTrackingUpdaterService(
	ILogger<AntiTrackingUpdaterService> logger,
	IConfiguration config,
	IAntiTrackingCore core)
	: BackgroundService
{
	private readonly TimeSpan _checkPeriod =
		TimeSpan.FromHours(config
			.GetRequiredSection(nameof(BotConfig))
			.Get<BotConfig>()!
			.UpdatePeriodHours);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Starting AntiTracking updater service");

		// Update the filters once before starting the loop
		await core.UpdateFiltersAsync(stoppingToken);

		using PeriodicTimer timer = new(_checkPeriod);
		while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
		{
			try
			{
				logger.LogInformation("Updating filters");
				await core.UpdateFiltersAsync(stoppingToken);
			}
			catch (Exception ex)
			{
				logger.LogError("Failed to update filters: {exception}", ex);
			}
		}

		logger.LogInformation("Stopping AntiTracking updater service");
	}
}
