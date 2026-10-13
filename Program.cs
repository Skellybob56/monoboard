using Raylib_cs;
using System.Runtime.InteropServices;
using static Raylib_cs.Raylib;
using static Monoboard.KeymapUtil;

namespace Monoboard;

class Program
{
	[DllImport("winmm.dll")]
	static extern uint timeBeginPeriod(uint uPeriod);
	[DllImport("winmm.dll")]
	static extern uint timeEndPeriod(uint uPeriod);

	Controller controller;
	MidiManager midiManager;
	Instrument instrument;
	Renderer renderer;

	static bool updateGraphics = false;

	private Program()
	{
		InitWindow(Renderer.screenWidth, Renderer.screenHeight, "Monoboard");
		SetExitKey(KeyboardKey.Null);

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
	public static void Main() => new Program().Start();

	void Start()
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
		timeEndPeriod(1);

		Renderer.Destroy();
		Instrument.Destroy();
		MidiManager.Destroy();
		Controller.Destroy();
		CloseWindow();
	}

	static public void ScheduleGraphicalUpdate()
	{
		updateGraphics = true;
	}
}
