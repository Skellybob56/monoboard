using static Monoboard.KeymapUtil;

namespace Monoboard;

class Instrument : Singleton<Instrument>
{
	public static Instrument Create(MidiManager midiManager)
	{ return Register(new Instrument(midiManager)); }

	// todo: add a smaller `checkTimeOffset` variant that only applies if there are no note keys pressed in the early window (in other words, check for no note keys pressed before the check time and start the rest early if detected)
	const double checkTimeOffset = 1d / 32d; // in seconds

	const int maxBaseOctave = 99;
	const int minBaseOctave = -9;

	readonly MidiManager midiManager;

	public static sbyte RootTone { get; private set; }

	public static int BaseOctave { get; private set; } = 4;
	public static int OctaveShift { get; private set; } = 0;
	int noteShift = 0;

	Keymap keymap;
	Keymap differenceKeymap;
	double? changeCheckTime;

	Keymap[] combinations;
	sbyte[] scale;

	private Instrument(MidiManager midiManager)
	{
		this.midiManager = midiManager;
		RootTone = 2;
		(combinations, scale) = InitCombinationsAndScale(8, "Default", "Major");
	}

	static (Keymap[] combinations, sbyte[] scale) InitCombinationsAndScale(int scaleSize, string combinationsFilename, string scaleFilename)
	{
		string sizePrefix = scaleSize.ToString() + " - ";
		Keymap[] combinations = FileReader.GetCombinations(sizePrefix + combinationsFilename);
		sbyte[] scale = FileReader.GetScale(sizePrefix + scaleFilename, combinations.Length);

		DebugOutputNoteGuide(combinations, scale);

		return (combinations, scale);
	}


	static void DebugOutputNoteGuide(ReadOnlySpan<Keymap> combinations, ReadOnlySpan<sbyte> scale)
	{
		for (int i = 0; i < combinations.Length; i++)
		{
			Console.WriteLine(DebugOutputOfSingleNote(combinations[i], RootTone + scale[i]));
		}

		string DebugOutputOfSingleNote(Keymap combination, int note)
		{
			// todo: use flat or sharp depending selected scale
			const string noteDisplay = "C C#D D#E F F#G G#A A#B ";

			// todo: generalize this as it assumes that noteInputMask is 1111 0000
			string combinationString = combination.KeymapToString()[..4];

			int tone = note;
			int octave = 4;
			while (tone < 0)
			{ tone += 12; octave -= 1; }
			while (tone >= 12)
			{ tone -= 12; octave += 1; }
			string toneString = noteDisplay.Substring(tone*2, 2);

			return new string(combinationString.Reverse().ToArray()) + $" {toneString}{octave} " + combinationString;
		}
	}

	public void Update(double time)
	{
		// todo: assert that the length of combinations is the same as the length of notes

		// update keymaps
		differenceKeymap = Controller.CurrentKeymap ^ keymap;
		keymap = Controller.CurrentKeymap;

		// update shifts
		noteShift = keymap.HasFlag(Keymap.Sharp)? 1 : 0;

		{
			int newOctaveShift = (keymap.HasFlag(Keymap.Up)? 1 : 0) + (keymap.HasFlag(Keymap.Down)? -1 : 0);
			if (OctaveShift != newOctaveShift) { Program.ScheduleGraphicalUpdate(); }
			OctaveShift = newOctaveShift;
		}

		// update base octave
		if (differenceKeymap.HasFlag(Keymap.ApplyOctave) && keymap.HasFlag(Keymap.ApplyOctave) && OctaveShift != 0)
		{
			BaseOctave += OctaveShift;
			BaseOctave = Math.Clamp(BaseOctave, minBaseOctave, maxBaseOctave);
			Program.ScheduleGraphicalUpdate();
		}

		UpdateNote(time);
	}

	void UpdateNote(double time)
	{
		if (changeCheckTime.HasValue)
		{
			if (changeCheckTime <= time)
			{
				int scaleIndex = Array.IndexOf(combinations, keymap & Keymap.Notes);
				if (scaleIndex == -1) // rest
				{
					// todo: put calls to midiManager somewhere more explicit
					midiManager.NoteEvent(null);
				}
				else
				{
					int note = RootTone + scale[scaleIndex] + noteShift;
					int octave = BaseOctave + OctaveShift;
					midiManager.NoteEvent((octave, note));
				}
				changeCheckTime = null;
			}
		}
		else
		{
			// todo: this shouldn't trigger on releasing Keymap.ApplyOctave
			if (differenceKeymap != Keymap.None)
			{ changeCheckTime = time + checkTimeOffset; } // if note changed
		}
	}
}
