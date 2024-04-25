using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AntiTrackingBot.Core.Models.FilterValue;
public class RegexFilterValue([StringSyntax(StringSyntaxAttribute.Regex)] string pattern) : IFilterValue
{
	private readonly Regex _regex = new(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

	public bool IsMatch(string input)
	{
		return _regex.IsMatch(input);
	}

	public override string ToString()
	{
		return $"/{pattern}/";
	}

	public override bool Equals(object? obj)
	{
		if (obj is RegexFilterValue other)
		{
			return ToString() == other.ToString();
		}
		return false;
	}

	public override int GetHashCode()
	{
		return pattern.GetHashCode();
	}
}
