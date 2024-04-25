using AntiTrackingBot.Core;
using AntiTrackingBot.Telegram.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AntiTrackingBot.Telegram.Services;
internal class BotService : BackgroundService
{
	private readonly ILogger<BotService> _logger;
	private readonly IAntiTrackingCore _core;
	private readonly TelegramBotClient _bot;

	private readonly InlineKeyboardMarkup _projectLinkMarkup;

	public BotService(ILogger<BotService> logger, IConfiguration config, IAntiTrackingCore core)
	{
		_logger = logger;
		_core = core;

		_logger.LogInformation($"Initializing {nameof(BotService)}");

		BotConfig botConfig = config.GetRequiredSection(nameof(BotConfig)).Get<BotConfig>()!;
		_bot = new TelegramBotClient(botConfig.TelegramKey);
		_projectLinkMarkup = new InlineKeyboardMarkup(
			new List<InlineKeyboardButton>
			{
				InlineKeyboardButton.WithUrl("Tracking Url Cleaned", botConfig.ProjectLinkUrl)
			});

		_logger.LogInformation($"{nameof(BotService)} Initialized");
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_bot.StartReceiving(HandleUpdateAsync, HandleError, cancellationToken: stoppingToken);
		_logger.LogInformation("Bot Start Receiving");
		return Task.CompletedTask;
	}

	private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
	{
		if (update.Message is null)
		{
			return;
		}

		Message message = update.Message;
		if (message.Type is not MessageType.Text && message.Text is null && message.Caption is null)
		{
			// Ignore non-text messages
			return;
		}

		string text = message.Text ?? message.Caption ?? string.Empty;
		try
		{
			if (_core.RemoveTracking(ref text))
			{
#if DEBUG
				_logger.LogInformation("Removed tracking from message: {Message}", text);
#else
				_logger.LogInformation("Removed tracking from message");
#endif

				await botClient.SendTextMessageAsync(
					chatId: message.Chat.Id,
					text: text,
					disableWebPagePreview: true,
					replyMarkup: _projectLinkMarkup,
					replyToMessageId: message.MessageId,
					cancellationToken: ct);
			}
		}
		catch (Exception e)
		{
#if DEBUG
			_logger.LogError(e, "Error while processing message: {Message}", text);
#else
			_logger.LogError(e, "Error while processing message");
#endif
		}
	}

	private Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
	{
		if (exception is ApiRequestException { ErrorCode: 502 })
		{
			// Ignore 502 errors because telegram just throws them randomly
			return Task.CompletedTask;
		}

		string errorMessage = exception switch
		{
			ApiRequestException apiRequestException
				=> $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
			_ => exception.ToString()
		};

		_logger.LogError(errorMessage);
		return Task.CompletedTask;
	}

}
