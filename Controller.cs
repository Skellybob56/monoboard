using Raylib_cs;
using static Raylib_cs.Raylib;
using static Monoboard.KeymapUtil;

namespace Monoboard;

class Controller : Singleton<Controller>
{
	public static bool MirrorMode { get; private set; } = false;
	public static Keymap CurrentKeymap { get; private set; } = Keymap.None;
	static Keymap differenceKeymap = Keymap.None;

	public static Controller Create()
	{ return Register(new Controller()); }

	private Controller() { }

	public void UpdateKeymap()
	{
		differenceKeymap = CurrentKeymap;
		CurrentKeymap = Keymap.None;

		if (!MirrorMode && IsKeyDown(KeyboardKey.A)) { CurrentKeymap |= Keymap.A; }
		if (IsKeyDown(KeyboardKey.S)) { CurrentKeymap |= MirrorMode? Keymap.Sharp : Keymap.S; }
		if (IsKeyDown(KeyboardKey.D)) { CurrentKeymap |= MirrorMode? Keymap.Down : Keymap.D; }
		if (IsKeyDown(KeyboardKey.F)) { CurrentKeymap |= MirrorMode? Keymap.Up : Keymap.F; }

		if (IsKeyDown(KeyboardKey.J)) { CurrentKeymap |= MirrorMode? Keymap.F : Keymap.Up; }
		if (IsKeyDown(KeyboardKey.K)) { CurrentKeymap |= MirrorMode? Keymap.D : Keymap.Down; }
		if (IsKeyDown(KeyboardKey.L)) { CurrentKeymap |= MirrorMode? Keymap.S : Keymap.Sharp; }
		if (MirrorMode && IsKeyDown(KeyboardKey.Semicolon)) { CurrentKeymap |= Keymap.A; }

		if (IsKeyDown(KeyboardKey.Space)) { CurrentKeymap |= Keymap.ApplyOctave; }

		differenceKeymap ^= CurrentKeymap;

		if (differenceKeymap != Keymap.None)
		{
			Program.ScheduleGraphicalUpdate();
		}
	}
}
