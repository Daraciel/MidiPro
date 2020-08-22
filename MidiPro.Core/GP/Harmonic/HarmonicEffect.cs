namespace MidiPro.Core.GP.Harmonic
{
    public abstract class HarmonicEffect
    {
        public float Fret { get; set; }

        public int Type { get; protected set; }

        protected HarmonicEffect(int type = 0)
        {
            Fret = 0;
            Type = type;
        }
    }
}