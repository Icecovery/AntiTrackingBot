using AntiTrackingBot.Core.Models.FilterValue;

namespace AntiTrackingBot.Core.Models.FilterRule;
public class SpecificFilterRule(IFilterValue domain, IFilterValue parameter) : IFilterRule
{
	public IFilterValue Domain { get; set; } = domain;
	public IFilterValue Parameter { get; set; } = parameter;

	public bool Filter(Url url)
	{
		if (!Domain.IsMatch(url.FullUrl))
		{
			return false;
		}

		string[] queryKeys = url.GetQueries();
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
		return $"Specific: [{Domain}] - {Parameter}";
	}

	public override bool Equals(object? obj)
	{
		if (obj is SpecificFilterRule other)
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
