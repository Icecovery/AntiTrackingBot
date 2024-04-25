namespace AntiTrackingBot.Core.Models.FilterRule;

public interface IFilterRule
{
	bool Filter(Url url);
}
