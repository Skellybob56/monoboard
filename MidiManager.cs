using System.Diagnostics;
using TEVirtualMidiBindingsCs;

namespace Monoboard;

class MidiManager : Singleton<MidiManager>
{
	public static MidiManager Create()
	{ return Register(new MidiManager()); }

	const string midiDeviceName = "monoboard";
	const byte noteVelocity = 0x7F; // 7 bit int max
	const bool startNoteBeforeEndNote = true; // when swapping between two notes, this will start the next note before ending the last which can prevent monophonic clicking

	byte? playingNote = null;

	private MidiManager()
	{
		MidiPortHandler.Initialize(midiDeviceName);
	}

	public void NoteEvent((int octave, int note)? noteData)
	{
		if (noteData is null)
		{ NoteOffEvent(); return; } // null noteData: treat as a rest

		int newNoteInt = noteData.Value.octave * 12 + noteData.Value.note;
		if (newNoteInt < 0x00 || newNoteInt > 0x7f)
		{ NoteOffEvent(); return; } // note out of range: treat as a rest

		NoteOnEvent((byte)newNoteInt);
	}

	void NoteOnEvent(byte newNote)
	{
		Debug.Assert(newNote < 0x7F);

		if (playingNote == newNote) { return; } // note is already playing

		if (!startNoteBeforeEndNote && playingNote is not null)
		{ TEVirtualMidi.virtualMIDISendData(MidiPortHandler.LpvmMidiPort, [0x80, playingNote.Value, 0x00], 3); }

		TEVirtualMidi.virtualMIDISendData(MidiPortHandler.LpvmMidiPort, [0x90, newNote, noteVelocity], 3);

		if (startNoteBeforeEndNote && playingNote is not null)
		{ TEVirtualMidi.virtualMIDISendData(MidiPortHandler.LpvmMidiPort, [0x80, playingNote.Value, 0x00], 3); }

		playingNote = newNote;
	}

	void NoteOffEvent()
	{
		if (playingNote is null)
		{ return; } // attempted to stop playing a note while no note was playing

		TEVirtualMidi.virtualMIDISendData(MidiPortHandler.LpvmMidiPort, [0x80, playingNote.Value, 0x00], 3);
		playingNote = null;
	}

	protected override void Dispose()
	{
		MidiPortHandler.Dispose();
	}
}
