using AntiTrackingBot.Core.Models.FilterValue;
using System.Collections.Generic;

namespace AntiTrackingBot.Core.Models.FilterRule;
public class GeneralFilterRule(IFilterValue parameter) : IFilterRule
{
	public IFilterValue Parameter { get; set; } = parameter;

	public bool Filter(Url url)
	{
		IEnumerable<string> queryKeys = url.GetQueries();
		bool changed = false;
		foreach (string key in queryKeys)
		{
			if (!Parameter.IsMatch(key))
			{
				continue;
			}

			url.RemoveQuery(key);
			changed = true;
		}

		return changed;
	}

	public override string ToString()
	{
		return $"General: {Parameter}";
	}

	public override bool Equals(object? obj)
	{
		if (obj is GeneralFilterRule other)
		{
			return ToString() == other.ToString();
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}
}
