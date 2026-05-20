
namespace Monoboard;

class Instrument : Singleton<Instrument>
{
    public static Instrument Create(MidiManager midiManager)
    { return Register(new Instrument(midiManager)); }

    const bool mirrorMode = true;
    const Keymap noteInputMask = Keymap.J | Keymap.K | Keymap.L | Keymap.C;
    const Keymap octaveShiftUpKey = Keymap.F;
    const Keymap octaveShiftDownKey = Keymap.D;
    const Keymap octaveApplyKey = Keymap.S;
    const double checkTimeOffset = 3d / 64d; // in seconds

    readonly MidiManager midiManager;

    int octave = 5;
    Keymap keymap;
    Keymap oldKeymap;
    Keymap differenceKeymap;
    double? changeCheckTime;
    bool octaveShiftedUp = false;
    bool octaveShiftedDown = false;

    Keymap[] combinations;
    sbyte[] notes;


    static void DebugOutputNoteGuide(ReadOnlySpan<Keymap> combinations, ReadOnlySpan<sbyte> notes)
    {
        string DebugOutputOfSingleNote(Keymap combination, sbyte note)
        {
            const string noteDisplay = "C C#D D#E F F#G G#A A#B ";

            // assumes that noteInputMask is 0000 1111
            string combinationString = combination.KeymapToString().Substring(4);

            int tone = note;
            int octave = 4;
            while (tone < 0)
            { tone += 12; octave -= 1; }
            while (tone >= 12)
            { tone -= 12; octave += 1; }
            string toneString = noteDisplay.Substring(tone*2, 2);

            return new string(combinationString.Reverse().ToArray()) + $" {toneString}{octave} " + combinationString;
        }

        for (int i = 0; i < combinations.Length; i++)
        {
            Console.WriteLine(DebugOutputOfSingleNote(combinations[i], notes[i]));
        }
    }

    static (Keymap[] combinations, sbyte[] notes) InitCombinationsAndScale(int scaleSize, string combinationsFilename, string notesFilename, int rootNote)
    {
        string sizePrefix = scaleSize.ToString() + " - ";
        Keymap[] combinations = FileReader.GetCombinations(sizePrefix + combinationsFilename);
        sbyte[] notes = FileReader.GetNotes(sizePrefix + notesFilename, rootNote, combinations.Length);

        DebugOutputNoteGuide(combinations, notes);

        return (combinations, notes);
    }

    private Instrument(MidiManager midiManager)
    {
        this.midiManager = midiManager;
        (combinations, notes) = InitCombinationsAndScale(8, "LC03", "Major", 7);
    }

    void UpdateOctave()
    {
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

    void UpdateNote(double time)
    {
        if (changeCheckTime.HasValue)
        {
            if (changeCheckTime <= time)
            {
                int noteIndex = Array.IndexOf(combinations, keymap & noteInputMask);
                if (noteIndex == -1) // rest
                {
                    midiManager.NoteEvent(null);
                }
                else
                {
                    midiManager.NoteEvent((octave, notes[noteIndex]));
                }
                changeCheckTime = null;
            }
        }
        else
        {
            if ((differenceKeymap & noteInputMask) != Keymap.None)
            { changeCheckTime = time + checkTimeOffset; } // if note changed
        }
    }


    public void Update(Keymap newKeymap, double time)
    {
        // todo: assert that the length of combinations is the same as the length of notes

        // update keymaps
        oldKeymap = keymap;
        keymap = mirrorMode? (Keymap)Utilities.ReverseBits((byte)newKeymap) : newKeymap;
        differenceKeymap = keymap ^ oldKeymap;

        UpdateOctave();

        UpdateNote(time);
    }
}
