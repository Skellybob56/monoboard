using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

class Renderer : Singleton<Renderer>
{
	public static Renderer Create()
	{ return Register(new Renderer()); }

	private Renderer() { }

	// inputs
	const int defaultMargin = 12;

	const int noteScaleWindowWidth = 64;

	const int logoHeight = Program.fontSize;

	const int keyBoxWidth = 48;
	const int keyBoxHeight = 48;
	const int keyBoxSpacing = 15;
	const int keyPressedSink = 6;
	static readonly Color keyColor = Color.White;
	static readonly Color keyPressedColor = Color.Gray;

	// maths
	const int logoTextX = defaultMargin + noteScaleWindowWidth;
	const int logoTextY = defaultMargin;

	const int keySetX = defaultMargin + noteScaleWindowWidth;
	const int keySetY = logoTextY + logoHeight + defaultMargin;
	const int keyBoxJump = keyBoxSpacing + keyBoxWidth;

	const int spacebarY = keySetY + keyBoxHeight + defaultMargin;
	const int spacebarWidth = 9*keyBoxWidth + 8*keyBoxSpacing;

	public void Render()
	{
		ClearBackground(Color.Black);
		DrawTextEx(Program.font, "MONOBOARD", new(logoTextX, logoTextY), Program.fontSize, 30, Color.White);

		DrawKey(keySetX + 0*keyBoxJump, keySetY, Glyph.A, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.A));
		DrawKey(keySetX + 1*keyBoxJump, keySetY, Glyph.S, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.S));
		DrawKey(keySetX + 2*keyBoxJump, keySetY, Glyph.D, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.D));
		DrawKey(keySetX + 3*keyBoxJump, keySetY, Glyph.F, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.F));
		// one key-sized gap here to split ASDF from JKL;
		DrawKey(keySetX + 5*keyBoxJump, keySetY, Glyph.J, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Up));
		DrawKey(keySetX + 6*keyBoxJump, keySetY, Glyph.K, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Down));
		DrawKey(keySetX + 7*keyBoxJump, keySetY, Glyph.L, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Sharp));
		DrawKey(keySetX + 8*keyBoxJump, keySetY, Glyph.Semicolon, false);

		DrawSpace(keySetX, spacebarY, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.ApplyOctave));
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
		DrawRectangleLines(x, y + yOffset, spacebarWidth, keyBoxHeight, pressed? keyPressedColor : keyColor);
	}
}