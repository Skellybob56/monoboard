using Raylib_cs;
using System.Runtime.InteropServices;
using static Raylib_cs.Raylib;
using static Monoboard.KeymapUtil;

namespace Monoboard;

internal static class Program
{
	[DllImport("winmm.dll")]
	static extern uint timeBeginPeriod(uint uPeriod);
	[DllImport("winmm.dll")]
	static extern uint timeEndPeriod(uint uPeriod);

	static Controller controller;
	static MidiManager midiManager;
	static Instrument instrument;

	static Program()
	{
		InitWindow(220, 40, "Monoboard");
		SetExitKey(KeyboardKey.Null);

		Font jetBrainsMonoFont = LoadFont("assets/JetBrainsMono-Regular.ttf");

		controller = Controller.Create();
		midiManager = MidiManager.Create();
		instrument = Instrument.Create(midiManager);

		// populate the screen buffer
		BeginDrawing();
		Render();
		EndDrawing();
	}

	// STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
	[System.STAThread]
	public static void Main()
	{
		timeBeginPeriod(1);
		while (!WindowShouldClose())
		{
			controller.UpdateKeymap();
			instrument.Update(GetTime());


			// end of tick
			Thread.Sleep(1); // place the smallest frequency reduction on the loop that i can to prevent the loop from consuming an entire cpu thread
			bool doRendering = false;
			if (doRendering)
			{ BeginDrawing(); Render(); EndDrawing(); } // EndDrawing handles PollInputEvents
			else { PollInputEvents(); }
		}
		timeEndPeriod(1);

		CloseWindow();
		MidiManager.Destroy();
	}

	static void Render()
	{
		ClearBackground(Color.Black);
		DrawTextEx(GetFontDefault(), "Monoboard", new(12, 12), 20, 12, Color.White);
	}
}
