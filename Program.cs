using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

internal static class Program
{
    // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
    [System.STAThread]
    public static void Main()
    {
        InitWindow(800, 480, "Hello World");
        
        while (!WindowShouldClose())
        {
            // todo: consider making these singletons and passing data between them explicitly for improved encapsulation
            Instrument.Update(Controller.GetKeymap());

            BeginDrawing();
            ClearBackground(Color.White);

            DrawText("Hello, world!", 12, 12, 20, Color.Black);

            EndDrawing();
        }

        CloseWindow();
        MidiManager.Dispose();
    }
}
