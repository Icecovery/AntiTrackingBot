using AntiTrackingBot.Core.Models;
using AntiTrackingBot.Core.Models.FilterRule;
using AntiTrackingBot.Core.Models.FilterValue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using urldetector.detection;

namespace AntiTrackingBot.Core;

public static partial class Utilities
{
	#region Helper Methods

	/// <summary>
	/// Downloads files from the specified URIs to the specified directory
	/// </summary>
	/// <param name="uris"> The URIs to download files from </param>
	/// <param name="to"> The directory to save the files to </param>
	/// <param name="ct"> The cancellation token </param>
	/// <returns> The paths to the downloaded files </returns>
	/// <exception cref="HttpRequestException"> If the file could not be downloaded </exception>
	public static async Task<string[]> DownloadFiles(Uri[] uris, string to, CancellationToken ct = default)
	{
		// Ensure the directory exists
		Directory.CreateDirectory(to);

		using HttpClient client = new();

		string[] files = new string[uris.Length];
		for (int index = 0; index < uris.Length; index++)
		{
			Uri uri = uris[index];

			// Download the file
			HttpResponseMessage response;
			try
			{
				response = await client.GetAsync(uri, ct);
				response.EnsureSuccessStatusCode();
			}
			catch (HttpRequestException e)
			{
				throw new HttpRequestException($"Failed to download file from {uri}", e);
			}

			// Write the file to disk
			string fileName = Path.GetFileName(uri.ToString());
			string filePath = Path.Combine(to, fileName);
			await using FileStream fs = new(filePath, FileMode.Create);
			await response.Content.CopyToAsync(fs, ct);

			files[index] = filePath;
		}

		return files;
	}

	/// <summary>
	/// Gets the MD5 hash of the specified stream
	/// </summary>
	/// <param name="stream"> The stream to hash </param>
	/// <returns> The Hex representation of the MD5 hash </returns>
	public static string GetHash(Stream stream)
	{
		using MD5 md5 = MD5.Create();
		byte[] hash = md5.ComputeHash(stream);
		return Convert.ToHexString(hash);
	}

	/// <summary>
	/// Extracts URLs from the specified text
	/// </summary>
	/// <param name="text"> The text to extract URLs from </param>
	/// <returns> The extracted URLs </returns>
	public static List<Url> GetUrls(string text)
	{
		UrlDetector detector = new(text, UrlDetectorOptions.Default);
		return detector.Detect().Select(url => new Url(url)).ToList();
	}

	/// <summary>
	/// Extracts the query parameter pairs from the specified query
	/// </summary>
	/// <param name="query"> The query to get the parameters from </param>
	/// <returns> The query parameter pairs </returns>
	public static List<string> GetQueryParameters(string query)
	{
		MatchCollection matches = QueryRegex().Matches(query);

		return matches.Select(match => match.Groups[1].Value).ToList();
	}

	#endregion

	#region Regex Patterns

	[GeneratedRegex("""removeparam=([^\,\n]+)""", RegexOptions.Compiled)]
	public static partial Regex FilterRemoveParamRegex();

	[GeneratedRegex("""domain=([^\,\n]+)|^(?:\|\|)?\^?([^\^\$]+)""", RegexOptions.Compiled)]
	public static partial Regex FilterDomainRegex();

	[GeneratedRegex("[?&]([^=]+=[^&]+)", RegexOptions.Compiled)]
	public static partial Regex QueryRegex();

	#endregion

	#region Regex Methods

	/// <summary>
	/// Checks if the specified input is a regex pattern (starts and ends with /)
	/// </summary>
	/// <example>
	/// <code>IsRegex("/example/") => true</code>
	/// </example>
	/// <param name="input"> The input to check </param>
	/// <returns> <see langword="true"/> if the input is a regex pattern, <see langword="false"/> otherwise </returns>
	public static bool IsRegex(string input)
	{
		return input.StartsWith('/') && input.EndsWith('/');
	}

	/// <summary>
	/// Gets the regex pattern from the specified input (removes the leading and trailing /)
	/// </summary>
	/// <param name="input"> The input to get the regex pattern from </param>
	/// <returns> The regex pattern </returns>
	public static string GetRegex(string input)
	{
		return input[1..^1];
	}

	/// <summary>
	/// Converts the specified domain with wildcards (<c>*</c>) to a regex pattern
	/// </summary>
	/// <param name="domain"> The domain to convert </param>
	/// <returns> The regex pattern surrounded by <c>/</c> or the domain if no wildcards are present </returns>
	public static string WildcardToRegex(string domain)
	{
		// Replace * with .*? to match any character 0 or more times non-greedy
		return domain.Contains('*') ? $"/{Regex.Escape(domain).Replace("\\*", ".*?")}/" : domain;
	}

	#endregion

	#region Filter File Processing

	/// <summary>
	/// Processes a line from a filter file
	/// </summary>
	/// <param name="line"> The line to process </param>
	/// <returns> The filter rules extracted from the line </returns>
	public static List<IFilterRule>? ProcessLine(string line)
	{
		// Skip empty lines and comments
		if (string.IsNullOrWhiteSpace(line) || line.StartsWith('!'))
			return null;

		if (!ExtractDomainAndParam(line, out string domain, out string param))
			return null;

		List<IFilterRule> rules = [];

		// split filter by |
		IEnumerable<IFilterValue> filterValues = IsRegex(param)
			? [new RegexFilterValue(GetRegex(param))]
			: param.Split('|').Select(x => new QueryFilterValue(x));

		if (string.IsNullOrWhiteSpace(domain))
		{
			// No domain, create a general rule
			rules.AddRange(filterValues.Select(filterValue => new GeneralFilterRule(filterValue)));
			return rules;
		}

		//// split domain by |
		//// domain might contain wildcards, convert them to regex
		IEnumerable<IFilterValue> domainValues = IsRegex(domain)
			? [new RegexFilterValue(GetRegex(domain))]
			: domain.Split('|')
				.Select(WildcardToRegex)
				.Select(x => IsRegex(x)
					? new RegexFilterValue(GetRegex(x)) as IFilterValue
					: new ContainsFilterValue(x));

		// Create a specific rule for each domain and filter combination
		rules.AddRange(domainValues
			.SelectMany(
				_ => filterValues,
				(domainValue, filterValue) => new SpecificFilterRule(domainValue, filterValue)));

		return rules;
	}

	/// <summary>
	/// Extracts the domain and parameter from the specified line
	/// </summary>
	/// <param name="line"> The line to extract the domain and parameter from </param>
	/// <param name="domain"> The extracted domain </param>
	/// <param name="param"> The extracted parameter </param>
	/// <returns> <see langword="true"/> if the domain and parameter were extracted, <see langword="false"/> otherwise </returns>
	public static bool ExtractDomainAndParam(string line, out string domain, out string param)
	{
		domain = string.Empty;
		param = string.Empty;

		Match matchParam = FilterRemoveParamRegex().Match(line);
		if (!matchParam.Success)
		{
			return false;
		}
		param = matchParam.Groups[1].Value;

		MatchCollection matchesDomain = FilterDomainRegex().Matches(line);

		switch (matchesDomain.Count)
		{
			case 0:
				break;
			case > 1: // We have multiple domains, take the second one (the one after the domain=)
			{
				domain = matchesDomain[1].Groups[1].Value;
				break;
			}
			default:
			{
				if (matchesDomain[0].Groups[1].Success)
				{
					domain = matchesDomain[0].Groups[1].Value;
				}
				else if (matchesDomain[0].Groups[2].Success)
				{
					domain = matchesDomain[0].Groups[2].Value;
				}
				break;
			}
		}

		return true;
	}

	#endregion
}
