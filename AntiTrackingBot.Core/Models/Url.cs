using System.Collections.Generic;

namespace AntiTrackingBot.Core.Models;
public class Url
{
	public urldetector.Url Value { get; }
	private readonly List<string> _queryParameters;

	public Url(urldetector.Url url)
	{
		Value = url;
		_queryParameters = Utilities.GetQueryParameters(Value.GetQuery());
	}

	public string FullUrl => Value.GetOriginalUrl();

	public string[] GetQueries()
	{
		return [.. _queryParameters];
	}

	public void RemoveQuery(string key)
	{
		_queryParameters.Remove(key);
	}

	public string Final()
	{
		if (_queryParameters.Count == 0)
		{
			return Value.GetOriginalUrl().Replace(Value.GetQuery(), string.Empty);
		}

		// Rebuild the URL with the new query parameters
		string query = "?" + string.Join("&", _queryParameters);
		return Value.GetOriginalUrl().Replace(Value.GetQuery(), query);
	}
}
