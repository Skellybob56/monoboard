using static Monoboard.KeymapUtil;

namespace Monoboard;

class Instrument : Singleton<Instrument>
{
	public static Instrument Create(MidiManager midiManager)
	{ return Register(new Instrument(midiManager)); }

	const double checkTimeOffset = 1d / 16d; // in seconds

	const Keymap noteInputMask = Keymap.A | Keymap.S | Keymap.D | Keymap.F;
	const Keymap octaveShiftUpKey = Keymap.Up;
	const Keymap octaveShiftDownKey = Keymap.Down;
	const Keymap sharpShiftKey = Keymap.Sharp;
	const Keymap octaveApplyKey = Keymap.ShiftOctave;

	readonly MidiManager midiManager;

	int octave = 5;
	Keymap keymap;
	Keymap differenceKeymap;
	double? changeCheckTime;
	bool octaveShiftedUp = false;
	bool octaveShiftedDown = false;
	bool sharpShifted = false;

	Keymap[] combinations;
	sbyte[] notes;

	private Instrument(MidiManager midiManager)
	{
		this.midiManager = midiManager;
		(combinations, notes) = InitCombinationsAndScale(8, "Default", "Dorian", 3);
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

		UpdateOctave();
		UpdateSharping();
		UpdateNote(time);
	}

	void UpdateOctave()
	{
		// todo: reduce repetiton
		if (differenceKeymap.HasFlag(octaveShiftUpKey))
		{
			if (keymap.HasFlag(octaveShiftUpKey) && !octaveShiftedUp)
			{
				octave += 1;
				octaveShiftedUp = true;
			}
			else if (octaveShiftedUp)
			{
				octave -= 1;
				octaveShiftedUp = false;
			}
		}
		if (differenceKeymap.HasFlag(octaveShiftDownKey))
		{
			if (keymap.HasFlag(octaveShiftDownKey) && !octaveShiftedDown)
			{
				octave -= 1;
				octaveShiftedDown = true;
			}
			else if (octaveShiftedDown)
			{
				octave += 1;
				octaveShiftedDown = false;
			}
		}
		if (differenceKeymap.HasFlag(octaveApplyKey) && keymap.HasFlag(octaveApplyKey))
		{
			octaveShiftedUp = false;
			octaveShiftedDown = false;
		}
	}

	void UpdateSharping()
	{
		if (differenceKeymap.HasFlag(sharpShiftKey))
		{
			sharpShifted = keymap.HasFlag(sharpShiftKey);
		}
	}

	void UpdateNote(double time)
	{
		if (changeCheckTime.HasValue)
		{
			if (changeCheckTime <= time)
			{
				int noteIndex = Array.IndexOf(combinations, keymap & noteInputMask);
				if (noteIndex == -1) // rest
				{
					// todo: put calls to midiManager somewhere more explicit
					midiManager.NoteEvent(null);
				}
				else
				{
					midiManager.NoteEvent((octave, notes[noteIndex] + (sharpShifted? 1 : 0)));
				}
				changeCheckTime = null;
			}
		}
		else
		{
			if ((differenceKeymap & (noteInputMask | Keymap.Sharp)) != Keymap.None)
			{ changeCheckTime = time + checkTimeOffset; } // if note changed
		}
	}
}
