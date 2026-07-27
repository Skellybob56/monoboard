using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

class Renderer : Singleton<Renderer>
{
	public static Renderer Create()
	{ return Register(new Renderer()); }

	private Renderer() { }


	const int keySet1Start = 8;
	const int keyBoxWidth = 32;
	const int keyBoxHeight = 32;
	const int keyBoxSpacing = 10;
	const int keySetSpacing = 24;
	const int keyPressedSink = 4;
	readonly Color keyColor = Color.White;
	readonly Color keyPressedColor = Color.Gray;

	const int keyBoxJump = keyBoxSpacing + keyBoxWidth;
	const int keySet2Start = keySet1Start + 4*keyBoxWidth + 3*keyBoxSpacing + keySetSpacing;
	const int fullKeysetWidth = 8*keyBoxWidth + 6*keyBoxSpacing + keySetSpacing;

	public void Render()
	{
		ClearBackground(Color.Black);
		DrawTextEx(Program.font, "MONOBOARD", new(10, 8), 32, 16, Color.White);

		DrawKey(keySet1Start + 0*keyBoxJump, 44, 'A', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.A));
		DrawKey(keySet1Start + 1*keyBoxJump, 44, 'S', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.S));
		DrawKey(keySet1Start + 2*keyBoxJump, 44, 'D', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.D));
		DrawKey(keySet1Start + 3*keyBoxJump, 44, 'F', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.F));

		DrawKey(keySet2Start + 0*keyBoxJump, 44, 'J', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Up));
		DrawKey(keySet2Start + 1*keyBoxJump, 44, 'K', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Down));
		DrawKey(keySet2Start + 2*keyBoxJump, 44, 'L', Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.Sharp));
		DrawKey(keySet2Start + 3*keyBoxJump, 44, ';', false);

		DrawSpace(keySet1Start, 84, Controller.CurrentKeymap.HasFlag(KeymapUtil.Keymap.ApplyOctave));
	}

	void DrawKey(int x, int y, char symbol, bool pressed)
	{
		int yOffset = pressed? keyPressedSink : 0;
		DrawRectangleLines(x, y + yOffset, keyBoxWidth, keyBoxHeight, pressed? keyPressedColor : keyColor);
		DrawTextCodepoint(Program.font, symbol, new (x, y + yOffset), 32, pressed? keyPressedColor : keyColor);
	}

	void DrawSpace(int x, int y, bool pressed)
	{
		int yOffset = pressed? keyPressedSink : 0;
		DrawRectangleLines(x, y + yOffset, fullKeysetWidth, keyBoxHeight, pressed? keyPressedColor : keyColor);
	}
}