namespace MidiPro.Core.GP
{
    public class TrillEffect
    {
        public int Fret { get; set; }
        public Duration Duration { get; set; }

        public TrillEffect()
        {
            Fret = 0;
            Duration = new Duration();
        }
    }
}