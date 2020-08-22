namespace MidiPro.Core.GP
{
    public class TimeSignature
    {
        public int Numerator { get; set; }
        public Duration Denominator { get; set; }
        public int[] Beams { get; set; }

        public TimeSignature()
        {
            Numerator = 4;
            Denominator = new Duration();
            Beams = new int[]{ 0, 0, 0, 0 };
        }
    }
}