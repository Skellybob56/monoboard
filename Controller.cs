using Raylib_cs;
using static Raylib_cs.Raylib;
using static Monoboard.KeymapUtil;

namespace Monoboard;

class Controller : Singleton<Controller>
{
	public static bool MirrorMode { get; private set; } = false;
	public static Keymap Keymap { get; private set; }

	public static Controller Create()
	{ return Register(new Controller()); }

	private Controller() { }

	public void UpdateKeymap()
	{
		Keymap = Keymap.None;

		if (!MirrorMode && IsKeyDown(KeyboardKey.A)) { Keymap |= Keymap.A; }
		if (IsKeyDown(KeyboardKey.S)) { Keymap |= MirrorMode? Keymap.Sharp : Keymap.S; }
		if (IsKeyDown(KeyboardKey.D)) { Keymap |= MirrorMode? Keymap.Down : Keymap.D; }
		if (IsKeyDown(KeyboardKey.F)) { Keymap |= MirrorMode? Keymap.Up : Keymap.F; }

		if (IsKeyDown(KeyboardKey.J)) { Keymap |= MirrorMode? Keymap.F : Keymap.Up; }
		if (IsKeyDown(KeyboardKey.K)) { Keymap |= MirrorMode? Keymap.D : Keymap.Down; }
		if (IsKeyDown(KeyboardKey.L)) { Keymap |= MirrorMode? Keymap.S : Keymap.Sharp; }
		if (MirrorMode && IsKeyDown(KeyboardKey.Semicolon)) { Keymap |= Keymap.A; }

		if (IsKeyDown(KeyboardKey.Space)) { Keymap |= Keymap.ApplyOctave; }
	}
}
