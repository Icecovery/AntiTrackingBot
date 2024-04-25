using AntiTrackingBot.Core;
using AntiTrackingBot.Core.Models;
using AntiTrackingBot.Test.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Xunit.Abstractions;

namespace AntiTrackingBot.Test.Models;

/// <summary>
/// This class is used to create a new instance of <see cref="AntiTrackingCore"/> for each test group
/// </summary>
public class CoreFixture : IDisposable
{
	private AntiTrackingCore? _core;
	private readonly string _tempPath =
		Path.Combine("tmp", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

	public AntiTrackingCore Create(ITestOutputHelper outputHelper, bool updateImmediately)
	{
		if (_core != null)
			return _core;

		ILogger<AntiTrackingCore> logger = CreateLogger<AntiTrackingCore>(outputHelper);
		IConfiguration config = CreateCoreConfig();

		_core = new AntiTrackingCore(logger, config);

		if (updateImmediately)
			_core.UpdateFiltersAsync().Wait();

		return _core;
	}

	public void Dispose()
	{
		if (Directory.Exists(_tempPath))
			Directory.Delete(_tempPath, true);

		GC.SuppressFinalize(this);
	}

	private static ILogger<T> CreateLogger<T>(ITestOutputHelper outputHelper)
	{
		ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.SetMinimumLevel(LogLevel.Debug);
			builder.AddProvider(new TestLoggerProvider(outputHelper));
		});
		ILogger<T> logger = loggerFactory.CreateLogger<T>();

		return logger;
	}

	private IConfiguration CreateCoreConfig()
	{
		// generate a random temporary path
		CoreConfig coreConfig = new()
		{
			Filters =
			[
				"https://raw.githubusercontent.com/AdguardTeam/AdguardFilters/master/TrackParamFilter/sections/specific.txt",
				"https://raw.githubusercontent.com/AdguardTeam/AdguardFilters/master/TrackParamFilter/sections/general_url.txt"
			],
			TemporaryPath = _tempPath
		};

		string configJson = JsonSerializer.Serialize(
			new Dictionary<string, object>
			{
				{ nameof(CoreConfig), coreConfig }
			});

		using MemoryStream stream = new(Encoding.UTF8.GetBytes(configJson));

		IConfiguration config = new ConfigurationBuilder()
			.AddJsonStream(stream)
			.Build();

		return config;
	}
}