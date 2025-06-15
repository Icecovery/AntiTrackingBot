using AntiTrackingBot.Core;
using AntiTrackingBot.Test.Models;

namespace AntiTrackingBot.Test;

/// <summary>
/// This group contains tests for the core functionality of the bot. See
/// <see cref="CoreUpdateTest"/> for the test that checks the
/// <see cref="AntiTrackingCore.UpdateFiltersAsync"/> method
/// </summary>
public class CoreTests(CoreFunctionalFixture fixture) : IClassFixture<CoreFunctionalFixture>
{
	#region Helpers
	private static void ShouldBeTrue(bool result)
	{
		Assert.True(result, "Did not remove tracking as expected");
	}

	private static void ShouldBeFalse(bool result)
	{
		Assert.False(result, "Removed tracking unexpectedly");
	}

	#endregion

	[Theory]
	// Remove tracking
	[InlineData("https://youtu.be/dQw4w9WgXcQ?si=ABCDEF", "https://youtu.be/dQw4w9WgXcQ")]
	// Remove tracking and keep fragment
	[InlineData("https://youtu.be/dQw4w9WgXcQ?si=ABCDEF#123", "https://youtu.be/dQw4w9WgXcQ#123")]
	// Remove tracking and keep other query parameters
	[InlineData("https://youtu.be/dQw4w9WgXcQ?si=ABCDEF&silly=this_is_fake", "https://youtu.be/dQw4w9WgXcQ?silly=this_is_fake")]
	// Remove tracking but keep other query parameters
	[InlineData("https://youtu.be/dQw4w9WgXcQ?si=ABCDEF&t=42", "https://youtu.be/dQw4w9WgXcQ?t=42")]
	[InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&pp=ABCDEFG&t=42s", "https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=42s")]
	// Remove multiple tracking
	[InlineData("https://twitter.com/example/status/1234567890?t=abcdefghij&s=19", "https://twitter.com/example/status/1234567890")]
	// Domain is regex
	[InlineData("https://weibo.com/test?weibo_id=1234&dt_dapp=1234", "https://weibo.com/test")]
	[InlineData("https://weibo.cn/test?weibo_id=1234&dt_dapp=1234", "https://weibo.cn/test")]
	// Query parameter is regex
	[InlineData("https://daraz.com/test?from=abc&abtest=123", "https://daraz.com/test")]
	[InlineData("https://daraz.com/test?from=abc&notabtest=123", "https://daraz.com/test?notabtest=123")]
	// Link within a message
	[InlineData("Hey check out this cool video I found: https://youtu.be/dQw4w9WgXcQ?si=ABCDEF&t=42, you will like it!",
				"Hey check out this cool video I found: https://youtu.be/dQw4w9WgXcQ?t=42, you will like it!")]
	// Multiple links within a message
	[InlineData("Hey check out this cool video I found: https://youtu.be/dQw4w9WgXcQ?si=ABCDEF&t=42, you will like it! Also, check out this tweet: https://twitter.com/example/status/1234567890?t=abcdefghij&s=19",
				"Hey check out this cool video I found: https://youtu.be/dQw4w9WgXcQ?t=42, you will like it! Also, check out this tweet: https://twitter.com/example/status/1234567890")]
	public void TestTracking(string input, string expected)
	{
		ShouldBeTrue(fixture.Core.RemoveTracking(ref input));

		Assert.Equal(expected, input);
	}

	[Theory]
	// No tracking
	[InlineData("https://twitter.com/example/status/1234567890")]
	[InlineData("https://youtu.be/dQw4w9WgXcQ")]
	// No tracking and keep fragment
	[InlineData("https://google.com/#1234")]
	// No tracking and keep other query parameters
	[InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
	[InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=42s")]
	public void TestNoTracking(string input)
	{
		string original = input;

		ShouldBeFalse(fixture.Core.RemoveTracking(ref input));

		Assert.Equal(original, input);
	}
}