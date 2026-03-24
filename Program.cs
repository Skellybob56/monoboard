using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Monoboard;

internal static class Program
{
    const int desiredFramerate = 250;
    const double desiredFrametime = 1d/desiredFramerate;

    static void Render()
    {
        BeginDrawing();
        ClearBackground(Color.Black);
        DrawTextEx(GetFontDefault(), "Monoboard", new(12, 12), 20, 12, Color.White);
        EndDrawing();
    }

    // STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
    [System.STAThread]
    public static void Main()
    {
        InitWindow(220, 40, "Monoboard");
        Font jetBrainsMonoFont = LoadFont("assets/JetBrainsMono-Regular.ttf");

        Controller controller = Controller.Create();
        MidiManager midiManager = MidiManager.Create();
        Instrument instrument = Instrument.Create(midiManager);

        // populate the screen buffer
        Render();

        while (!WindowShouldClose())
        {
            double frameStartTime = GetTime();
            Keymap latestKeymap = controller.GetKeymap();
            instrument.Update(latestKeymap, GetTime());

            bool renderNeeded = false; // replace when dynamic picture elements are added
            if (renderNeeded)
            { Render(); }
            else
            {
                double timeTillNextFrame = desiredFrametime - (GetTime() - frameStartTime);
                if (timeTillNextFrame < 0d) { continue; }
                WaitTime(timeTillNextFrame);
                PollInputEvents();
            }
        }

        CloseWindow();
        midiManager.Dispose();
    }
}
