using TEVirtualMidiBindingsCs;

namespace Monoboard;

class MidiManager : Singleton<MidiManager>
{
    public static MidiManager Create()
    { return Register(new MidiManager()); }

    const string midiDeviceName = "monoboard";
    const byte noteVelocity = 0x7F; // 7 bit int max

    byte? playingNoteNumber;

    private MidiManager()
    {
        MidiPortHandler.Initialize(midiDeviceName);
    }
    
    public void NoteEvent((int octave, int note)? noteData)
    {
        if (noteData is null)
        { NoteEventDirect(null); return; } // null noteData: treat as a rest

        int newNoteNumberInt = noteData.Value.octave * 12 + noteData.Value.note;
        if (newNoteNumberInt < 0x00 || newNoteNumberInt > 0x7f)
        { NoteEventDirect(null); return; } // note out of range: treat as a rest

        NoteEventDirect((byte)newNoteNumberInt);
    }

    void NoteEventDirect(byte? newNoteNumber)
    {
        if (newNoteNumber is null)
        {
            // note off
            if (playingNoteNumber is null)
            { return; } // attempted to stop playing a note while no note was playing
            NoteOffEvent(playingNoteNumber.Value);
            playingNoteNumber = null;
            return;
        }

        if (playingNoteNumber == newNoteNumber) { return; } // note is already playing

        // note on
        if (playingNoteNumber is not null)
        { NoteOffEvent(playingNoteNumber.Value); }

        playingNoteNumber = newNoteNumber;
        NoteOnEvent(playingNoteNumber.Value, noteVelocity);
    }

    // todo: ensure safety when note or velocity are above 0x7F
    void NoteOnEvent(byte note, byte velocity)
    {
        TEVirtualMidi.virtualMIDISendData(MidiPortHandler.LpvmMidiPort, [0x90, note, velocity], 3);
    }
    void NoteOffEvent(byte note)
    {
        TEVirtualMidi.virtualMIDISendData(MidiPortHandler.LpvmMidiPort, [0x80, note, 0x00], 3);
    }

    protected override void Dispose()
    {
        MidiPortHandler.Dispose();
    }
}
