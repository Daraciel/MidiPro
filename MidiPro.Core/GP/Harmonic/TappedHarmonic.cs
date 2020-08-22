namespace MidiPro.Core.GP.Harmonic
{
    public class TappedHarmonic : HarmonicEffect
    {
        public TappedHarmonic(int fret = 0) : base(3)
        {
            Fret = fret;
        }
    }
}

