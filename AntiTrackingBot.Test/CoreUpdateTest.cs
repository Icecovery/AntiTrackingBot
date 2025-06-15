using AntiTrackingBot.Core;
using AntiTrackingBot.Test.Models;
using System;
using System.Threading.Tasks;

namespace AntiTrackingBot.Test;

/// <summary>
/// This group is only used to test the <see cref="AntiTrackingCore.UpdateFiltersAsync"/> method.
/// See <see cref="CoreTests"/> for the rest of the tests
/// </summary>
public class CoreUpdateTest(CoreUpdateFixture updateFixture) : IClassFixture<CoreUpdateFixture>
{
	[Fact]
	public async Task RemoveTrackingTest()
	{
		Exception? ex = await Record.ExceptionAsync(() => updateFixture.Core.UpdateFiltersAsync());

		Assert.Null(ex);
	}
}