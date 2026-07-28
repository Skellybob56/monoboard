using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

class Renderer : Singleton<Renderer>
{
	public static Renderer Create()
	{ return Register(new Renderer()); }

	private Renderer() { }

	const int noteScaleWindowWidth = 64;

	const int logoTextPositionX = 15 + noteScaleWindowWidth;
	const int logoTextPositionY = 12;

	const int keySet1Start = 12 + noteScaleWindowWidth;
	const int keyBoxWidth = 48;
	const int keyBoxHeight = 48;
	const int keyBoxSpacing = 15;
	const int keyPressedSink = 6;
	readonly Color keyColor = Color.White;
	readonly Color keyPressedColor = Color.Gray;

	const int keyBoxJump = keyBoxSpacing + keyBoxWidth;
	const int keySet2Start = keySet1Start + 5*keyBoxJump;
	const int fullKeysetWidth = 9*keyBoxWidth + 8*keyBoxSpacing;

	public void Render()
	{
		ClearBackground(Color.Black);
		DrawTextEx(Program.font, "MONOBOARD", new(logoTextPositionX, logoTextPositionY), Program.fontSize, 30, Color.White);

		DrawKey(keySet1Start + 0*keyBoxJump, 72, Glyph.A, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.A));
		DrawKey(keySet1Start + 1*keyBoxJump, 72, Glyph.S, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.S));
		DrawKey(keySet1Start + 2*keyBoxJump, 72, Glyph.D, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.D));
		DrawKey(keySet1Start + 3*keyBoxJump, 72, Glyph.F, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.F));

		DrawKey(keySet2Start + 0*keyBoxJump, 72, Glyph.J, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Up));
		DrawKey(keySet2Start + 1*keyBoxJump, 72, Glyph.K, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Down));
		DrawKey(keySet2Start + 2*keyBoxJump, 72, Glyph.L, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Sharp));
		DrawKey(keySet2Start + 3*keyBoxJump, 72, Glyph.Semicolon, false);

		DrawSpace(keySet1Start, 132, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.ApplyOctave));
	}

	void DrawKey(int x, int y, Glyph glyph, bool pressed)
	{
		int yOffset = pressed? keyPressedSink : 0;
		DrawRectangleLines(x, y + yOffset, keyBoxWidth, keyBoxHeight, pressed? keyPressedColor : keyColor);
		DrawGlyph(x, y + yOffset, glyph, pressed? keyPressedColor : keyColor);
	}

	enum Glyph : byte
	{
		A, S, D, F,
		J, K, L, Semicolon
	}

	static char GlyphToChar(Glyph glyph)
	{
		return glyph switch
		{
			Glyph.A => 'A', Glyph.S => 'S', Glyph.D => 'D', Glyph.F => 'F',
			Glyph.J => 'J', Glyph.K => 'K', Glyph.L => 'L', Glyph.Semicolon => ';',
			_ => throw new ArgumentException("Glyph enum value not recognized.", nameof(glyph))
		};
	}

	static void DrawGlyph(int x, int y, Glyph glyph, Color color)
	{
		char character = GlyphToChar(glyph);

		Vector2 offset = glyph switch
		{
			Glyph.A => new(8, 2),
			Glyph.S => new(11, 2),
			Glyph.D => new(7, 2),
			Glyph.F => new(10, 2),
			Glyph.J => new(18, -1),
			Glyph.K => new(8, 2),
			Glyph.L => new(9, 2),
			Glyph.Semicolon => new(18, -2),
			_ => throw new ArgumentException("Glyph enum value not recognized.", nameof(glyph))
		};

		DrawTextCodepoint(Program.font, character, new Vector2(x, y) + offset, Program.fontSize, color);
	}

	void DrawSpace(int x, int y, bool pressed)
	{
		int yOffset = pressed? keyPressedSink : 0;
		DrawRectangleLines(x, y + yOffset, fullKeysetWidth, keyBoxHeight, pressed? keyPressedColor : keyColor);
	}
}