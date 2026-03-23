using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

internal static class Program
{
    // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
    [System.STAThread]
    public static void Main()
    {
        InitWindow(200, 60, "Monoboard");
        Font jetBrainsMonoFont = LoadFont("assets/JetBrainsMono-Regular.ttf");

        while (!WindowShouldClose())
        {
            // todo: consider making these singletons and passing data between them explicitly for improved encapsulation
            Instrument.Update(Controller.GetKeymap(), GetTime());

            BeginDrawing();
            ClearBackground(Color.Black);

            DrawFPS(8, 8);
            DrawTextEx(GetFontDefault(), "Monoboard", new(16, 32), 20, 10, Color.White);

            EndDrawing();
        }

        CloseWindow();
        MidiManager.Dispose();
    }
}
