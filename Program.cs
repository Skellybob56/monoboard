using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

internal static class Program
{
    static readonly OutputDevice output = OutputDevice.GetByName("monoboard");

    static void MidiUpdate()
    {
        if (IsKeyPressed(KeyboardKey.J)) { output.SendEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100)); }
        if (IsKeyReleased(KeyboardKey.J)) { output.SendEvent(new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0)); }
    }

    // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
    [System.STAThread]
    public static void Main()
    {
        InitWindow(800, 480, "Hello World");
        
        while (!WindowShouldClose())
        {
            MidiUpdate();

            BeginDrawing();
            ClearBackground(Color.White);

            DrawText("Hello, world!", 12, 12, 20, Color.Black);

            EndDrawing();
        }

        CloseWindow();
        output.Dispose();
    }
}
