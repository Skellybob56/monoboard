using Raylib_cs;
using System.Runtime.InteropServices;
using static Raylib_cs.Raylib;
using static Monoboard.KeymapUtil;

namespace Monoboard;

class Program : IDisposable
{
	[DllImport("winmm.dll")]
	static extern uint timeBeginPeriod(uint uPeriod);
	[DllImport("winmm.dll")]
	static extern uint timeEndPeriod(uint uPeriod);

	readonly Controller controller;
	readonly MidiManager midiManager;
	readonly Instrument instrument;
	readonly Renderer renderer;

	static bool updateGraphics = false;

	private Program()
	{
		InitWindow(Renderer.screenWidth, Renderer.screenHeight, "Monoboard");
		SetExitKey(KeyboardKey.Null);

		controller = new();
		midiManager = new();
		instrument = new(midiManager);
		renderer = new();

		// populate the screen buffer
		BeginDrawing();
		renderer.Render();
		EndDrawing();
	}

	// STAThread is required if you deploy using NativeAOT on Windows - See https://github.com/raylib-cs/raylib-cs/issues/301
	[System.STAThread]
	public static void Main() { using Program p = new(); p.Run(); }

	void Run()
	{
		Image logo = LoadImage("assets/logo.png");
		SetWindowIcon(logo);
		UnloadImage(logo);

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
	}

	static public void ScheduleGraphicalUpdate()
	{
		updateGraphics = true;
	}

	public void Dispose()
	{
		timeEndPeriod(1);

		renderer.Dispose();
		instrument.Dispose();
		midiManager.Dispose();
		controller.Dispose();

		CloseWindow();
	}
}
