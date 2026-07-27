using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

class Renderer : Singleton<Renderer>
{
	public static Renderer Create()
	{ return Register(new Renderer()); }

	private Renderer() { }

	const int keySet1Start = 12;
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
		DrawTextEx(Program.font, "MONOBOARD", new(15, 12), Program.fontSize, 30, Color.White);

		DrawKey(keySet1Start + 0*keyBoxJump, 72, 'A', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.A));
		DrawKey(keySet1Start + 1*keyBoxJump, 72, 'S', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.S));
		DrawKey(keySet1Start + 2*keyBoxJump, 72, 'D', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.D));
		DrawKey(keySet1Start + 3*keyBoxJump, 72, 'F', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.F));

		DrawKey(keySet2Start + 0*keyBoxJump, 72, 'J', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Up));
		DrawKey(keySet2Start + 1*keyBoxJump, 72, 'K', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Down));
		DrawKey(keySet2Start + 2*keyBoxJump, 72, 'L', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Sharp));
		DrawKey(keySet2Start + 3*keyBoxJump, 72, ';', false);

		DrawSpace(keySet1Start, 132, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.ApplyOctave));
	}

	void DrawKey(int x, int y, char glyph, bool pressed)
	{
		int yOffset = pressed? keyPressedSink : 0;
		DrawRectangleLines(x, y + yOffset, keyBoxWidth, keyBoxHeight, pressed? keyPressedColor : keyColor);
		DrawCentredGlyph(new (x + keyBoxWidth/2, y + keyBoxHeight/2 + yOffset), glyph, pressed? keyPressedColor : keyColor);
	}

	void DrawCentredGlyph(Vector2 centre, char glyph, Color color)
	{
		Vector2 glyphSize = MeasureTextEx(Program.font, new string(glyph, 1), Program.fontSize, 0);
		DrawTextCodepoint(Program.font, glyph, centre - Vector2.Round(glyphSize/2f), Program.fontSize, color);
	}

	void DrawSpace(int x, int y, bool pressed)
	{
		int yOffset = pressed? keyPressedSink : 0;
		DrawRectangleLines(x, y + yOffset, fullKeysetWidth, keyBoxHeight, pressed? keyPressedColor : keyColor);
	}
}