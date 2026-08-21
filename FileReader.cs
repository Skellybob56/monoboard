using System.Diagnostics;
using static Monoboard.KeymapUtil;

namespace Monoboard;

static class FileReader
{
	const string patternFolder = "assets/patterns/";
	const string patternExtension = ".monpa";

	const string whitespace = "\t\r\n ,";

	public static (Keymap[] combinations, sbyte[] scale) GetCombinationsAndScale(string filename)
	{
		string path = patternFolder + filename + patternExtension;
		string pattern = File.ReadAllText(path);

		List<Keymap> combinations = new(16);
		List<sbyte> scale = new(16);

		int cursor = 0;
		ushort seenKeymaps = 0x0000; // bitmask for the 16 possible keymaps. high bit is !!!!, low bit is .... (technically invalid)
		while (true)
		{
			Keymap? combination = TryReadKeymap(pattern, ref cursor);
			if (combination is null) { break; }
			else
			{
				// check for keymap repetitions
				Debug.Assert((ushort)combination.Value < 16); // pure note combinations should be 0 to 15
				ushort keymapCode = (ushort)(1 << (ushort)combination.Value);
				if ((seenKeymaps & keymapCode) != 0) { throw new FormatException($"Duplicate combination, '{combination.Value}' found at character {cursor-4}."); }
				seenKeymaps |= keymapCode;

				combinations.Add(combination.Value);

				scale.Add(ReadSByte(pattern, ref cursor));
			}
		}

		Debug.Assert(combinations.Count == scale.Count);

		return (combinations.ToArray(), scale.ToArray());
	}

	static Keymap? TryReadKeymap(string pattern, ref int cursor)
	{
		const char keyPressedChar = '!';
		const char keyUnpressedChar = '.';

		SkipWhitespace(pattern, ref cursor);
		if (cursor >= pattern.Length) { return null; }

		Keymap keymap = Keymap.None;
		foreach (Keymap key in keymapNoteKeys)
		{
			if (cursor >= pattern.Length) { throw new FormatException("EOF occurred while reading a keymap."); }

			char symbol = pattern[cursor];

			if (symbol == keyPressedChar)
			{
				keymap |= key;
			}
			else if (symbol != keyUnpressedChar)
			{
				throw new FormatException($"'{symbol}' found at character {cursor} where a keymap was expected. Keymaps can only use '{keyPressedChar}' for pressed keys and '{keyUnpressedChar}' for unpressed keys.");
			}

			cursor++;
		}

		if (keymap == Keymap.None) { throw new FormatException($"The keymap at character {cursor-4} is empty ('{new string(keyUnpressedChar, 4)}'). Keymaps are not allowed to be empty"); }

		return keymap;
	}

	static sbyte ReadSByte(string pattern, ref int cursor)
	{
		SkipWhitespace(pattern, ref cursor);
		if (cursor >= pattern.Length) { throw new FormatException("The final keymap combination is not followed by a note integer."); }

		int integerStartPosition = cursor; // only used for later exceptions

		bool negative = pattern[cursor] == '-';
		if (negative) { cursor++; SkipWhitespace(pattern, ref cursor); }

		const int earlyBreakSize = ushort.MaxValue;
		if (!char.IsBetween(pattern[cursor], '0', '9')) { throw new FormatException($"Digit expected at character {cursor} instead of '{pattern[cursor]}'"); }

		int value = 0;
		while (char.IsBetween(pattern[cursor], '0', '9'))
		{
			int digit = pattern[cursor] - '0';
			value *= 10;
			value += digit;

			if (value >= earlyBreakSize) { break; } // prevent extremely long numbers from overflowing the integer - this is already big enough to throw an error later

			cursor++;
		}

		value = negative? -value : value;

		if (value > sbyte.MaxValue) { throw new FormatException($"The note integer '{value}' at character {integerStartPosition} is greater than {sbyte.MaxValue}. Note integers must be less than {sbyte.MaxValue}"); }
		if (value < sbyte.MinValue) { throw new FormatException($"The note integer '{value}' at character {integerStartPosition} is less than {sbyte.MinValue}. Note integers must be greater than {sbyte.MinValue}"); }

		return (sbyte)value;
	}

	static void SkipWhitespace(string pattern, ref int cursor)
	{
		while (cursor < pattern.Length && whitespace.Contains(pattern[cursor])) { cursor++; }
	}
}
