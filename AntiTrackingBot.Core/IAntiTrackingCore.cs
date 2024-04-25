using System.Threading;
using System.Threading.Tasks;

namespace AntiTrackingBot.Core;
public interface IAntiTrackingCore
{
	/// <summary>
	/// Update filters defined in the configuration
	/// </summary>
	/// <param name="ct"> The cancellation token </param>
	/// <returns></returns>
	Task UpdateFiltersAsync(CancellationToken ct = default);

	/// <summary>
	/// Remove tracking from the link in the given text
	/// </summary>
	/// <param name="text"> The input text </param>
	/// <returns> <see langword="true"/> if the tracking was removed, <see langword="false"/> otherwise </returns>
	bool RemoveTracking(ref string text);
}
