using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace Monoboard;

class MidiManager : Singleton<MidiManager>
{
    public static MidiManager Create()
    { return Register(new MidiManager()); }

    private MidiManager() { }

    readonly SevenBitNumber noteVelocity = (SevenBitNumber)127;
    readonly OutputDevice output = OutputDevice.GetByName("monoboard");

    SevenBitNumber? playingNoteNumber;
    
    public void NoteEvent((int octave, int note)? noteData)
    {
        // todo: add testing to ensure octave and note are within legal ranges
        if (!noteData.HasValue)
        {
            if (!playingNoteNumber.HasValue)
            { return; } // Attempted to stop playing a note while no note was playing
            output.SendEvent(new NoteOffEvent(playingNoteNumber.Value, (SevenBitNumber)0));
            playingNoteNumber = null;
            return;
        }

        if (playingNoteNumber.HasValue)
        { output.SendEvent(new NoteOffEvent(playingNoteNumber.Value, (SevenBitNumber)0)); }

        SevenBitNumber noteNumber = (SevenBitNumber)(byte)(noteData.Value.octave * 12 + noteData.Value.note);
        output.SendEvent(new NoteOnEvent(noteNumber, noteVelocity));
        playingNoteNumber = noteNumber;
    }

    public void Dispose()
    {
        output.Dispose();
    }
}
