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
	public AntiTrackingCore Core { get; }
	private readonly string _tempPath =
		Path.Combine("tmp", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

	/// <summary>
	/// Initializes a new instance of the <see cref="CoreFixture"/> class
	/// </summary>
	/// <param name="diagnosticMessageSink"> The diagnostic message sink for logging </param>
	/// <param name="updateFilter"> Indicates whether to update the filters before running the tests </param>
	public CoreFixture(IMessageSink diagnosticMessageSink, bool updateFilter)
	{
		ILogger<AntiTrackingCore> logger = CreateLogger<AntiTrackingCore>(diagnosticMessageSink);
		IConfiguration config = CreateCoreConfig();

		Core = new AntiTrackingCore(logger, config);
		if (updateFilter)
		{
			Core.UpdateFiltersAsync().Wait();
		}
	}

	public void Dispose()
	{
		if (Directory.Exists(_tempPath))
			Directory.Delete(_tempPath, true);

		GC.SuppressFinalize(this);
	}

	private static ILogger<T> CreateLogger<T>(IMessageSink diagnosticMessageSink)
	{
		ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
		{
			builder.SetMinimumLevel(LogLevel.Debug);
			builder.AddProvider(new TestLoggerProvider(diagnosticMessageSink));
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

/// <summary>
/// This fixture is used to create a new instance of <see cref="AntiTrackingCore"/> for each test group,
/// and it updates the filters before running the tests.
/// </summary>
public class CoreFunctionalFixture(IMessageSink diagnosticMessageSink) : CoreFixture(diagnosticMessageSink, true);

/// <summary>
/// This fixture is used to create a new instance of <see cref="AntiTrackingCore"/> for each test group,
/// and it does not update the filters before running the tests.
/// </summary>
public class CoreUpdateFixture(IMessageSink diagnosticMessageSink) : CoreFixture(diagnosticMessageSink, false);
