using AntiTrackingBot.Core;
using AntiTrackingBot.Test.Models;
using System;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace AntiTrackingBot.Test;

/// <summary>
/// This group is only used to test the <see cref="AntiTrackingCore.UpdateFiltersAsync"/> method.
/// See <see cref="CoreTests"/> for the rest of the tests
/// </summary>
/// <param name="fixture"></param>
/// <param name="outputHelper"></param>
public class CoreUpdateTest(CoreFixture fixture, ITestOutputHelper outputHelper)
	: IClassFixture<CoreFixture>
{
	private readonly AntiTrackingCore _core = fixture.Create(outputHelper, false);

	[Fact]
	public async Task RemoveTrackingTest()
	{
		Exception? ex = await Record.ExceptionAsync(() => _core.UpdateFiltersAsync());

		Assert.Null(ex);
	}
}