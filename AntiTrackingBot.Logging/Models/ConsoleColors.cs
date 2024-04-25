using System;

namespace AntiTrackingBot.Logging.Models;
public readonly struct ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
{
	public ConsoleColor? Foreground { get; } = foreground;
	public ConsoleColor? Background { get; } = background;

	public ConsoleColors() : this(null, null) { }

	public ConsoleColors(ConsoleColor foreground) : this(foreground, null) { }
}
