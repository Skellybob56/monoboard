namespace Monoboard;

static class Utilities
{
    public static byte ReverseBits(byte v)
    {
        byte r = v; // r will be reversed bits of v; first get LSB of v
        int s = 7; // extra shift needed at end
        for (v >>= 1; v != 0; v >>= 1)
        {
            r <<= 1;
            r |= (byte)(v & 1);
            s--;
        }
        r <<= s; // shift when v's highest bits are zero
        return r;
    }
}
