using TEVirtualMidiBindingsCs;

namespace Monoboard;

class MidiManager : Singleton<MidiManager>
{
    public static MidiManager Create()
    { return Register(new MidiManager()); }

    const string midiDeviceName = "monoboard";
    readonly byte noteVelocity = 0x7F; // 7 bit int max

    private MidiManager()
    {
        MidiPortHandler.Initialize(midiDeviceName);
    }

    byte? playingNoteNumber;
    
    public void NoteEvent((int octave, int note)? noteData)
    {
        // todo: add testing to ensure octave and note are within legal ranges
        if (!noteData.HasValue)
        {
            if (!playingNoteNumber.HasValue)
            { return; } // attempted to stop playing a note while no note was playing
            NoteOffEvent(playingNoteNumber.Value);
            playingNoteNumber = null;
            return;
        }

        if (playingNoteNumber.HasValue)
        { NoteOffEvent(playingNoteNumber.Value); }

        playingNoteNumber = (byte)((noteData.Value.octave * 12 + noteData.Value.note) & 0x7F);
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
