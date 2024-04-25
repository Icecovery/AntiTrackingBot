namespace AntiTrackingBot.Core.Models.FilterValue;

public class QueryFilterValue(string value) : IFilterValue
{
	public bool IsMatch(string input)
	{
		// split input by '=' and take the first part to compare
		return input.Split('=')[0] == value;
	}

	public override string ToString()
	{
		return $"\"{value}\"";
	}

	public override bool Equals(object? obj)
	{
		if (obj is QueryFilterValue other)
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
