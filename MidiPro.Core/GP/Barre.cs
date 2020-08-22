namespace MidiPro.Core.GP
{
    public class Barre
    {
        public int Start { get; set; }

        public int End { get; set; }

        public int Fret { get; set; }

        private Barre()
        {
            Start = 0;
            End = 0;
            Fret = 0;
        }

        public Barre(int fret = 0, int start = 0, int end = 0) : this()
        {
            Start = start; 
            Fret = fret; 
            End = end;
        }

        public int[] Range()
        {
            return new int[] { Start, End };
        }
    }
}