using TEVirtualMidiBindingsCs;

namespace Monoboard;

class MidiPortHandler : IDisposable
{
	public bool PortIsNull => LpvmMidiPort == nint.Zero;
	public nint LpvmMidiPort { get; private set; } = nint.Zero;

	public MidiPortHandler(string portName)
	{
		// create midi port with virtualMIDICreatePortEx2 from teVirtualMIDI64.dll
		LpvmMidiPort = TEVirtualMidi.virtualMIDICreatePortEx2(portName, nint.Zero, nint.Zero, 65535, (uint)TEVirtualMidi.VmFlags.InstantiateTX);

		if (PortIsNull) { throw new InvalidOperationException($"{nameof(LpvmMidiPort)} is a null after attempting to create the port."); }
	}

	public void Dispose()
	{
		if (PortIsNull) { return; }
		
		TEVirtualMidi.virtualMIDIClosePort(LpvmMidiPort);
		LpvmMidiPort = nint.Zero;
	}
}
