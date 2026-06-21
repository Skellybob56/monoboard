using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

[Flags]
enum Keymap : ushort
{
	None = 0x0000,
	A = 0x0001,
	S = 0x0002,
	D = 0x0004,
	F = 0x0008,
	J = 0x0010,
	K = 0x0020,
	L = 0x0040,
	C = 0x0080, // semicolon
	P = 0x0100  // space
}

class Controller : Singleton<Controller>
{
	const bool mirrorMode = true;

	public static Controller Create()
	{ return Register(new Controller()); }

	private Controller() { }

	public Keymap GetKeymap()
	{
		Keymap keymap = Keymap.None;

		if (IsKeyDown(KeyboardKey.A)) { keymap |= mirrorMode? Keymap.C : Keymap.A; }
		if (IsKeyDown(KeyboardKey.S)) { keymap |= mirrorMode? Keymap.L : Keymap.S; }
		if (IsKeyDown(KeyboardKey.D)) { keymap |= mirrorMode? Keymap.K : Keymap.D; }
		if (IsKeyDown(KeyboardKey.F)) { keymap |= mirrorMode? Keymap.J : Keymap.F; }

		if (IsKeyDown(KeyboardKey.J)) { keymap |= mirrorMode? Keymap.F : Keymap.J; }
		if (IsKeyDown(KeyboardKey.K)) { keymap |= mirrorMode? Keymap.D : Keymap.K; }
		if (IsKeyDown(KeyboardKey.L)) { keymap |= mirrorMode? Keymap.S : Keymap.L; }
		if (IsKeyDown(KeyboardKey.Semicolon)) { keymap |= mirrorMode? Keymap.A : Keymap.C; }

		if (IsKeyDown(KeyboardKey.Space)) { keymap |= Keymap.P; }

		return keymap;
	}
}
