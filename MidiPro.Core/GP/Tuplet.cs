namespace MidiPro.Core.GP
{
    public class Tuplet
    {
        public int Enters { get; set; }
        public int Times { get; set; }

        public Tuplet()
        {
            Enters = 1;
            Times = 1;
        }

        public Tuplet(int enters, int times)
        {
            Enters = enters;
            Times = times;
        }

        public int ConvertTime(int time)
        {
            return (int)(time * (float)Times / (float)Enters);
        }
    }
}