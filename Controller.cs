using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

[Flags]
enum Keymap : byte
{
    None = 0x00,
    A = 0x01,
    S = 0x02,
    D = 0x04,
    F = 0x08,
    J = 0x10,
    K = 0x20,
    L = 0x40,
    C = 0x80
}

static class Controller
{
    public static Keymap keymap;

    public static void Update()
    {
        keymap = Keymap.None;

        if (IsKeyDown(KeyboardKey.A)) { keymap |= Keymap.A; }
        if (IsKeyDown(KeyboardKey.S)) { keymap |= Keymap.S; }
        if (IsKeyDown(KeyboardKey.D)) { keymap |= Keymap.D; }
        if (IsKeyDown(KeyboardKey.F)) { keymap |= Keymap.F; }

        if (IsKeyDown(KeyboardKey.J)) { keymap |= Keymap.J; }
        if (IsKeyDown(KeyboardKey.K)) { keymap |= Keymap.K; }
        if (IsKeyDown(KeyboardKey.L)) { keymap |= Keymap.L; }
        if (IsKeyDown(KeyboardKey.Semicolon)) { keymap |= Keymap.C; }
    }
}
