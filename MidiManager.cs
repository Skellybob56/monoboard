using TEVirtualMidiBindingsCs;

namespace Monoboard;

class MidiManager : Singleton<MidiManager>
{
	public static MidiManager Create()
	{ return Register(new MidiManager()); }

	const string midiDeviceName = "monoboard";
	const byte noteVelocity = 0x7F; // 7 bit int max

	byte? playingNote;

	private MidiManager()
	{
		MidiPortHandler.Initialize(midiDeviceName);
	}

	public void NoteEvent((int octave, int note)? noteData)
	{
		if (noteData is null)
		{ NoteEventDirect(null); return; } // null noteData: treat as a rest

		int newNoteInt = noteData.Value.octave * 12 + noteData.Value.note;
		if (newNoteInt < 0x00 || newNoteInt > 0x7f)
		{ NoteEventDirect(null); return; } // note out of range: treat as a rest

		NoteEventDirect((byte)newNoteInt);
	}

	void NoteEventDirect(byte? newNote)
	{
		if (newNote is null)
		{
			// note off
			if (playingNote is null)
			{ return; } // attempted to stop playing a note while no note was playing
			NoteOffEvent(playingNote.Value);
			playingNote = null;
			return;
		}

		if (playingNote == newNote) { return; } // note is already playing

		// note on
		if (playingNote is not null)
		{ NoteOffEvent(playingNote.Value); }

		playingNote = newNote;
		NoteOnEvent(playingNote.Value, noteVelocity);
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
