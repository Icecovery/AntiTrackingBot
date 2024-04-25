namespace AntiTrackingBot.Core.Models.FilterValue;

public class ContainsFilterValue(string value) : IFilterValue
{
	public bool IsMatch(string input)
	{
		return input.Contains(value);
	}

	public override string ToString()
	{
		return $"\"{value}\"";
	}

	public override bool Equals(object? obj)
	{
		if (obj is ContainsFilterValue other)
		{
			return ToString() == other.ToString();
		}
		return false;
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}
}
