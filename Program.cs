using Raylib_cs;
using System.Runtime.InteropServices;
using static Raylib_cs.Raylib;

namespace Monoboard;

internal static class Program
{
    [DllImport("winmm.dll")]
    static extern uint timeBeginPeriod(uint uPeriod);
    [DllImport("winmm.dll")]
    static extern uint timeEndPeriod(uint uPeriod);

    static void Render()
    {
        ClearBackground(Color.Black);
        DrawTextEx(GetFontDefault(), "Monoboard", new(12, 12), 20, 12, Color.White);
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
        BeginDrawing();
        Render();
        EndDrawing();

        timeBeginPeriod(1);
        while (!WindowShouldClose())
        {
            Keymap latestKeymap = controller.GetKeymap();
            instrument.Update(latestKeymap, GetTime());


            // end of tick
            Thread.Sleep(1); // place the smallest frequency reduction on the loop that i can to prevent the loop from consuming an entire cpu thread
            bool renderFrame = false;
            if (renderFrame)
            { BeginDrawing(); Render(); EndDrawing(); } // EndDrawing handles PollInputEvents
            else { PollInputEvents(); }
        }
        timeEndPeriod(1);

        CloseWindow();
        midiManager.Dispose();
    }
}
