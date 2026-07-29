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
	static Renderer renderer;

	public static readonly Font font;
	public const int fontSize = 48;

	static bool updateGraphics = false;

	static Program()
	{
		InitWindow(Renderer.screenWidth, Renderer.screenHeight, "Monoboard");
		SetExitKey(KeyboardKey.Null);

		font = LoadFontEx("assets/LibreBaskerville-VariableFont_wght.ttf", fontSize,
			['M', 'O', 'N', 'B', 'A', 'R', 'D', // MONOBOARD
			 'S', 'F', 'J', 'K', 'L', ';', // ASDF JKL;
			 'C', 'E', 'G', '#', 'b'], 13); // A B C D E F G # b

		controller = Controller.Create();
		midiManager = MidiManager.Create();
		instrument = Instrument.Create(midiManager);
		renderer = Renderer.Create();

		// populate the screen buffer
		BeginDrawing();
		renderer.Render();
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
			if (updateGraphics)
			{ BeginDrawing(); renderer.Render(); EndDrawing(); updateGraphics = false; } // EndDrawing handles PollInputEvents
			else { PollInputEvents(); }
		}
		timeEndPeriod(1);

		CloseWindow();
		Controller.Destroy();
		MidiManager.Destroy();
		Instrument.Destroy();
		Renderer.Destroy();
	}

	public static void ScheduleGraphicalUpdate()
	{
		updateGraphics = true;
	}
}
