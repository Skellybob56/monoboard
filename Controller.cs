using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

// todo: make this a struct to centralise computation
[Flags]
enum Keymap : byte
{
	None = 0x00,

	A = 0x01,
	S = 0x02,
	D = 0x04,
	F = 0x08,

	Up = 0x10,
	Down = 0x20,
	Sharp = 0x40,
	ShiftOctave = 0x80
}

class Controller : Singleton<Controller>
{
	const bool mirrorMode = false;

	public static Controller Create()
	{ return Register(new Controller()); }

	private Controller() { }

	public Keymap GetKeymap()
	{
		Keymap keymap = Keymap.None;

		if (!mirrorMode && IsKeyDown(KeyboardKey.A)) { keymap |= Keymap.A; }
		if (IsKeyDown(KeyboardKey.S)) { keymap |= mirrorMode? Keymap.Sharp : Keymap.S; }
		if (IsKeyDown(KeyboardKey.D)) { keymap |= mirrorMode? Keymap.Down : Keymap.D; }
		if (IsKeyDown(KeyboardKey.F)) { keymap |= mirrorMode? Keymap.Up : Keymap.F; }

		if (IsKeyDown(KeyboardKey.J)) { keymap |= mirrorMode? Keymap.F : Keymap.Up; }
		if (IsKeyDown(KeyboardKey.K)) { keymap |= mirrorMode? Keymap.D : Keymap.Down; }
		if (IsKeyDown(KeyboardKey.L)) { keymap |= mirrorMode? Keymap.S : Keymap.Sharp; }
		if (mirrorMode && IsKeyDown(KeyboardKey.Semicolon)) { keymap |= Keymap.A; }

		if (IsKeyDown(KeyboardKey.Space)) { keymap |= Keymap.ShiftOctave; }

		return keymap;
	}
}
