using static Monoboard.KeymapUtil;

namespace Monoboard;

class Instrument : Singleton<Instrument>
{
	public static Instrument Create(MidiManager midiManager)
	{ return Register(new Instrument(midiManager)); }

	// todo: add a smaller `checkTimeOffset` variant that only applies if there are no note keys pressed in the early window (in other words, check for no note keys pressed before the check time and start the rest early if detected)
	const double checkTimeOffset = 1d / 32d; // in seconds

	readonly MidiManager midiManager;

	int baseOctave = 4;
	int octaveShift = 0;
	int noteShift = 0;

	Keymap keymap;
	Keymap differenceKeymap;
	double? changeCheckTime;

	Keymap[] combinations;
	sbyte[] notes;

	private Instrument(MidiManager midiManager)
	{
		this.midiManager = midiManager;
		(combinations, notes) = InitCombinationsAndScale(8, "Default", "Major", 2);
	}

	static (Keymap[] combinations, sbyte[] notes) InitCombinationsAndScale(int scaleSize, string combinationsFilename, string notesFilename, int rootNote)
	{
		string sizePrefix = scaleSize.ToString() + " - ";
		Keymap[] combinations = FileReader.GetCombinations(sizePrefix + combinationsFilename);
		sbyte[] notes = FileReader.GetNotes(sizePrefix + notesFilename, rootNote, combinations.Length);

		DebugOutputNoteGuide(combinations, notes);

		return (combinations, notes);
	}


	static void DebugOutputNoteGuide(ReadOnlySpan<Keymap> combinations, ReadOnlySpan<sbyte> notes)
	{
		for (int i = 0; i < combinations.Length; i++)
		{
			Console.WriteLine(DebugOutputOfSingleNote(combinations[i], notes[i]));
		}

		string DebugOutputOfSingleNote(Keymap combination, sbyte note)
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

	public void Update(Keymap newKeymap, double time)
	{
		// todo: assert that the length of combinations is the same as the length of notes

		// update keymaps
		differenceKeymap = newKeymap ^ keymap;
		keymap = newKeymap;

		// update shifts
		noteShift = keymap.HasFlag(Keymap.Sharp)? 1 : 0;
		octaveShift = (keymap.HasFlag(Keymap.Up)? 1 : 0) + (keymap.HasFlag(Keymap.Down)? -1 : 0);

		// update base octave
		if (differenceKeymap.HasFlag(Keymap.ApplyOctave) && keymap.HasFlag(Keymap.ApplyOctave))
		{
			baseOctave += octaveShift;
		}

		UpdateNote(time);
	}

	void UpdateNote(double time)
	{
		if (changeCheckTime.HasValue)
		{
			if (changeCheckTime <= time)
			{
				int noteIndex = Array.IndexOf(combinations, keymap & Keymap.Notes);
				if (noteIndex == -1) // rest
				{
					// todo: put calls to midiManager somewhere more explicit
					midiManager.NoteEvent(null);
				}
				else
				{
					int note = notes[noteIndex] + noteShift;
					int octave = baseOctave + octaveShift;
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
