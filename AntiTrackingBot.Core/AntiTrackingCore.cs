using AntiTrackingBot.Core.Models;
using AntiTrackingBot.Core.Models.FilterRule;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntiTrackingBot.Core;
public class AntiTrackingCore : IAntiTrackingCore
{
	private readonly ILogger<AntiTrackingCore> _logger;
	private readonly CoreConfig _config;

	private readonly Dictionary<string, string> _fileHashCache = [];
	private readonly Dictionary<string, List<IFilterRule>> _filterCache = [];

	public AntiTrackingCore(ILogger<AntiTrackingCore> logger, IConfiguration config)
	{
		_logger = logger;

		_logger.LogInformation($"Initializing {nameof(AntiTrackingCore)}");

		_config = config.GetRequiredSection(nameof(CoreConfig)).Get<CoreConfig>()!;

#if DEBUG
		_logger.LogInformation(_config.ToString());
#endif

		_logger.LogInformation($"{nameof(AntiTrackingCore)} Initialized");
	}

	public async Task UpdateFiltersAsync(CancellationToken ct = default)
	{
		// Get the files that need to be downloaded
		_logger.LogInformation("Updating filters:\n\t{filters}", string.Join("\n\t", _config.Filters));
		Uri[] uris = _config.Filters.Select(filter => new Uri(filter)).ToArray();

		// Download the files
		string[] files = await Utilities.DownloadFiles(uris, _config.TemporaryPath, ct);
		_logger.LogInformation("Downloaded {length} files", files.Length);

		// Process each file
		foreach (string file in files)
		{
			_logger.LogInformation("Processing {file}", file);
			await ProcessFile(file, ct);
		}

		_logger.LogInformation("Filters updated, total {length} rules from {files} files",
			_filterCache.Values.Sum(rules => rules.Count),
			files.Length);

		_logger.LogDebug(DumpRules());
	}

	#region File Processing

	private async Task ProcessFile(string file, CancellationToken ct)
	{
		await using FileStream fs = File.Open(file, FileMode.Open);

		// check if the file has been updated, if not skip
		if (!TryUpdateHash(file, fs))
			return;

		_logger.LogInformation("Reading {file}", file);

		List<IFilterRule> rules = [];

		// Read the file line by line
		using StreamReader sr = new(fs);
		while (await sr.ReadLineAsync(ct) is { } line)
		{
			List<IFilterRule>? result = Utilities.ProcessLine(line);
			if (result is not null)
			{
				rules.AddRange(result);
			}
		}

		// Add the rules to the cache
		_filterCache[file] = rules;

		_logger.LogInformation("Added {length} rules from {file}", rules.Count, file);
	}

	private bool TryUpdateHash(string file, Stream fs)
	{
		// Calculate the hash of the file
		string hash = Utilities.GetHash(fs);
		// Reset the stream position
		fs.Seek(0, SeekOrigin.Begin);

		// Check if the hash is in the cache
		if (!_fileHashCache.TryGetValue(file, out string? oldHash))
		{
			// Add the hash to the cache if it is not found
			_logger.LogInformation("File hash ({hash}) not found in cache, adding", hash);
			_fileHashCache.Add(file, hash);
		}
		else
		{
			// Check if the hash has changed
			if (oldHash == hash)
			{
				_logger.LogInformation("File hash ({hash}) has not changed, skipping", hash);
				return false; // Skip the file if the hash has not changed
			}

			// Update the hash in the cache if it has changed
			_logger.LogInformation("File hash ({hash}) has changed, updating", hash);
			_fileHashCache[file] = hash;
		}

		return true;
	}

	#endregion

	public bool RemoveTracking(ref string text)
	{
		// Extract the URL from the text
		List<Url> urls = Utilities.GetUrls(text);

		if (urls.Count == 0)
		{
			// No URLs found
			_logger.LogDebug("No URLs found in {text}", text);
			return false;
		}

		bool changed = false;
		foreach (Url url in urls)
		{
			int hit = _filterCache
				.SelectMany(file => file.Value)
				.Count(rule => rule.Filter(url));

			if (hit is 0)
				break;

			_logger.LogDebug("Replacing URL from {url} to {final}", url.FullUrl, url.Final());
			text = text.Replace(url.FullUrl, url.Final());
			changed = true;
		}

		_logger.LogDebug("Changed: {changed}, new text: {text}", changed, text);

		return changed;
	}

	public string DumpRules()
	{
		StringBuilder sb = new();
		foreach (KeyValuePair<string, List<IFilterRule>> fileCache in _filterCache)
		{
			sb.AppendLine($"File: {fileCache.Key}");
			foreach (IFilterRule rule in fileCache.Value)
			{
				sb.AppendLine($"\t{rule}");
			}

			sb.AppendLine();
		}
		return sb.ToString();
	}
}
