namespace Monoboard;

static class KeymapUtil
{
	[Flags]
	public enum Keymap : byte
	{
		None = 0x00,

		A = 0x01,
		S = 0x02,
		D = 0x04,
		F = 0x08,

		Up = 0x10,
		Down = 0x20,
		Sharp = 0x40,
		ApplyOctave = 0x80,

		Notes = 0x0f,
		Modifiers = 0x70,
		NoteModifers = Notes | Modifiers
	}

	public static string KeymapToString(this Keymap keymap)
	{
		int index = 0;
		char[] output = new char[8];
		for (byte i = 1; i != 0; i <<= 1)
		{
			output[index++] = keymap.HasFlag((Keymap)i) ? '!' : '.';
		}

		return new string(output);
	}
}
