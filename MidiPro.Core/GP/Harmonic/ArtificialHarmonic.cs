using MidiPro.Core.BE;
using MidiPro.Core.GP.Enums;

namespace MidiPro.Core.GP.Harmonic
{
    public class ArtificialHarmonic : HarmonicEffect
    {
        public PitchClass Pitch;
        public Octaves Octave;

        public ArtificialHarmonic(PitchClass pitch = null, Octaves octave = 0) : base(2)
        {
            Pitch = pitch;
            Octave = octave;
        }
    }
}