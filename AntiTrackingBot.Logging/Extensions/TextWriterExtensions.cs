using AntiTrackingBot.Logging.Models;
using System;
using System.IO;

namespace AntiTrackingBot.Logging.Extensions;
internal static class TextWriterExtensions
{
	internal const string ResetColor = "\x1B[0m";

	internal static string GetBackgroundColorEscapeCode(ConsoleColor color)
	{
		return color switch
		{
			ConsoleColor.Black => "\x1B[40m",
			ConsoleColor.DarkRed => "\x1B[41m",
			ConsoleColor.DarkGreen => "\x1B[42m",
			ConsoleColor.DarkYellow => "\x1B[43m",
			ConsoleColor.DarkBlue => "\x1B[44m",
			ConsoleColor.DarkMagenta => "\x1B[45m",
			ConsoleColor.DarkCyan => "\x1B[46m",
			ConsoleColor.Gray => "\x1B[47m",
			_ => "\x1B[40m"
		};
	}

	internal static string GetForegroundColorEscapeCode(ConsoleColor color)
	{
		return color switch
		{
			ConsoleColor.Black => "\x1B[30m",
			ConsoleColor.DarkRed => "\x1B[31m",
			ConsoleColor.DarkGreen => "\x1B[32m",
			ConsoleColor.DarkYellow => "\x1B[33m",
			ConsoleColor.DarkBlue => "\x1B[34m",
			ConsoleColor.DarkMagenta => "\x1B[35m",
			ConsoleColor.DarkCyan => "\x1B[36m",
			ConsoleColor.Gray => "\x1B[37m",
			ConsoleColor.Red => "\x1B[1m\x1B[31m",
			ConsoleColor.Green => "\x1B[1m\x1B[32m",
			ConsoleColor.Yellow => "\x1B[1m\x1B[33m",
			ConsoleColor.Blue => "\x1B[1m\x1B[34m",
			ConsoleColor.Magenta => "\x1B[1m\x1B[35m",
			ConsoleColor.Cyan => "\x1B[1m\x1B[36m",
			ConsoleColor.White => "\x1B[1m\x1B[37m",
			_ => "\x1B[1m\x1B[37m"
		};
	}

	public static void WriteColoredMessage(this TextWriter textWriter, string message, ConsoleColors consoleColors)
	{
		// Order: backgroundcolor, foregroundcolor, Message, reset foregroundcolor, reset backgroundcolor
		if (consoleColors.Background.HasValue)
		{
			textWriter.Write(GetBackgroundColorEscapeCode(consoleColors.Background.Value));
		}
		if (consoleColors.Foreground.HasValue)
		{
			textWriter.Write(GetForegroundColorEscapeCode(consoleColors.Foreground.Value));
		}
		textWriter.Write(message);
		textWriter.Write(ResetColor); // reset color
	}
}
